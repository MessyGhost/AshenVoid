using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.ECS.FSM;
using Terraria;
using static AshenVoid.Core.ECS.BehaviorTree.NodeBuilder;
using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.Intents;
using AshenVoid.Core.ECS.Interfaces;
using Microsoft.Xna.Framework;
using System;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase1State : IState
    {
        private readonly PhaseConfig _config;
        private Node _behaviorTree;
        private int _patrolDirection = 1;

        // Context fields, populated by Enter/Update
        private ComponentController _controller;
        private NPC _npc;
        private Player _target;
        private AIStateComponent _aiState; // For properties like ShouldSummon

        public Phase1State(PhaseConfig config)
        {
            _config = config;
        }

        public void Enter(ComponentController controller, NPC npc)
        {
            _controller = controller;
            _npc = npc;
            _aiState = controller.GetComponent<AIStateComponent>();
            _behaviorTree = BuildBehaviorTree();
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
            // A helper function to ensure target is not null before accessing it
            Func<Vector2> safeTargetCenter = () => _target != null ? _target.Center : _npc.Center;

            return Fallback(
                // Highest priority: Transition to Phase 2
                Sequence(
                    new ConditionNode(() => _npc.life < _npc.lifeMax * 0.5f),
                    new ActionNode(() =>
                    {
                        _aiState.ChangeState(new Phase2State()); // Placeholder for now
                        return NodeState.Success;
                    })
                ),

                // High priority: Chase player if too far
                Sequence(
                    new ConditionNode(() => _target != null && _npc.Distance(_target.Center) > 650f),
                    AIBehaviorFactory.SetChase(_controller, safeTargetCenter, 320f)
                ),

                // Summon logic
                Sequence(
                    new ConditionNode(() => _aiState.ShouldSummon),
                    AIBehaviorFactory.SetSpawnNpc(_controller, _config.Summon.NpcId, () => safeTargetCenter() + new Vector2(0, 1000)),
                    new ActionNode(() => { _aiState.ShouldSummon = false; return NodeState.Success; })
                ),

                // Dash logic
                Sequence(
                    new ConditionNode(() => _aiState.ShouldDash),
                    // Charge up
                    AIBehaviorFactory.SetChase(_controller, () => _npc.Center + (_npc.Center - safeTargetCenter()).SafeNormalize(Vector2.UnitX) * 100, 0),
                    Wait(_config.Dash.ChargeTime),
                    // Dash
                    AIBehaviorFactory.SetChase(_controller, safeTargetCenter, 0), // This should be a high-speed chase
                    new ActionNode(() => { _aiState.ResetDashTrigger(); return NodeState.Success; })
                ),

                // Default patrol and attack logic
                PatrolBehavior()
            );
        }

        private Node PatrolBehavior()
        {
            return Sequence(
                // Move left and right above the player
                new ActionNode(() =>
                {
                    if (_target == null) return NodeState.Failure;

                    var targetPos = _target.Center + new Vector2(300 * _patrolDirection, -300);
                    _controller.GetComponent<IMovementComponent>().SetIntent(new ChaseIntent(targetPos, 50f));

                    // Check for turnaround
                    if (Math.Abs(_npc.Center.X - targetPos.X) < 100f)
                    {
                        _patrolDirection *= -1; // Reverse direction

                        // Fire projectile on turnaround
                        var attackComponent = _controller.GetComponent<IAttackComponent>();
                        if (attackComponent.IsReady())
                        {
                            attackComponent.SetIntent(new ShootProjectileIntent(_target.Center, _config.Attacks.BasicShot));
                        }
                    }
                    return NodeState.Success;
                })
            );
        }
    }
}