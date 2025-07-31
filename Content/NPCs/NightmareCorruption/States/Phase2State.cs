using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Interfaces;
using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase2State : IState
    {
        private readonly PhaseConfig _config;
        private Node _behaviorTree;
        private int _orbitDirection = 1;

        // Context
        private ComponentController _controller;
        private NPC _npc;
        private Player _target;
        private AIStateComponent _aiState;

        public Phase2State(PhaseConfig config)
        {
            _config = config;
        }

        public void Enter(ComponentController controller, NPC npc)
        {
            _controller = controller;
            _npc = npc;
            _aiState = controller.GetComponent<AIStateComponent>();
            _behaviorTree = BuildBehaviorTree();

            // Potentially change music or visual effects for phase 2
        }

        public void Update(ComponentController controller, NPC npc, Player target)
        {
            _controller = controller;
            _npc = npc;
            _target = target;

            _behaviorTree?.Evaluate();
        }

        public void Exit()
        {
            _behaviorTree = null;
        }

        private Node BuildBehaviorTree()
        {
            Func<Vector2> safeTargetCenter = () => _target != null && _target.active ? _target.Center : _npc.Center;

            return new FallbackNode(
                // Highest priority: Transition to Death state if health is critical
                new SequenceNode(
                    new ConditionNode(() => _npc.life <= 1),
                    new ActionNode(() =>
                    {
                        _aiState.ChangeState(new DeathState());
                        return NodeState.Success;
                    })
                ),

                // Aggressive teleport and shoot attack
                new SequenceNode(
                    AIBehaviorFactory.IsAttackReady(_controller),
                    new ActionNode(() =>
                    {
                        // Teleport to a random position near the player
                        var randomOffset = new Vector2(Main.rand.Next(-400, 400), Main.rand.Next(-400, -200));
                        AIBehaviorFactory.SetTeleport(_controller, () => safeTargetCenter() + randomOffset);
                        return NodeState.Success;
                    }),
                    new WaitNode(0.2f), // Brief pause after teleport
                    AIBehaviorFactory.SetShootProjectile(_controller, safeTargetCenter, _config.Attacks.BasicShot)
                ),

                // Default behavior: Orbit the player
                new ActionNode(() =>
                {
                    if (_target == null || !_target.active)
                    {
                        AIBehaviorFactory.SetIdle(_controller);
                        return NodeState.Failure;
                    }

                    AIBehaviorFactory.SetOrbit(_controller, safeTargetCenter, 350f, _orbitDirection);
                    return NodeState.Success;
                })
            );
        }
    }
}