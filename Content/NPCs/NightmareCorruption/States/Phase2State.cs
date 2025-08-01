using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Interfaces;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase2State : IState
    {
        private PhaseConfig _config;
        private Node _behaviorTree;
        private int _orbitDirection = 1;
        private Blackboard _blackboard;

        public void Enter(Blackboard blackboard)
        {
            _blackboard = blackboard;
            // Assuming Phase2 uses Phase1 config for now, as in the original code
            _config = _blackboard.Get<BossConfig>("BossConfig").Phase1;
            _behaviorTree = BuildBehaviorTree();
        }

        public void Update(Blackboard blackboard)
        {
            _blackboard = blackboard;
            _behaviorTree?.Evaluate();
        }

        public void Exit(Blackboard blackboard)
        {
            _behaviorTree = null;
            _blackboard = null;
            _config = null;
        }

        private Node BuildBehaviorTree()
        {
            return new FallbackNode(
                // Highest priority: Transition to Death state if health is critical
                new SequenceNode(
                    new ConditionNode(() => _blackboard.Get<NPC>(BlackboardKeys.NPC).life <= 1),
                    new ActionNode(() =>
                    {
                        _blackboard.Get<AIStateComponent>(BlackboardKeys.AIState).ChangeState<DeathState>();
                        return NodeState.Success;
                    })
                ),

                // Aggressive teleport and shoot attack
                new SequenceNode(
                    new ConditionNode(() => true), // Simplified attack ready check
                    new ActionNode(() =>
                    {
                        var target = _blackboard.Get<Player>(BlackboardKeys.Target);
                        var controller = _blackboard.Get<ComponentController>(BlackboardKeys.Controller);
                        var randomOffset = new Vector2(Main.rand.Next(-400, 400), Main.rand.Next(-400, -200));
                        controller.GetComponent<IMovementComponent>()?.SetIntent(new Core.ECS.Intents.TeleportIntent(target.Center + randomOffset));
                        return NodeState.Success;
                    }),
                    new WaitNode(0.2f), // Brief pause after teleport
                    new ActionNode(() =>
                    {
                        Terraria.ModLoader.ModContent.GetInstance<AshenVoid>().Logger.Info("Shooting projectile would happen here.");
                        return NodeState.Success;
                    })
                ),

                // Default behavior: Orbit the player
                new ActionNode(() =>
                {
                    var target = _blackboard.Get<Player>(BlackboardKeys.Target);
                    var controller = _blackboard.Get<ComponentController>(BlackboardKeys.Controller);

                    if (target == null || !target.active)
                    {
                        controller.GetComponent<IMovementComponent>()?.SetIntent(new Core.ECS.Intents.IdleIntent());
                        return NodeState.Failure;
                    }

                    controller.GetComponent<IMovementComponent>()?.SetIntent(new Core.ECS.Intents.OrbitIntent(target.Center, 350f, _orbitDirection));
                    return NodeState.Success;
                })
            );
        }
    }
}