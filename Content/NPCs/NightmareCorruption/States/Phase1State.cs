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
        private AIComponent _ai;
        private int _patrolDirection = 1;

        public Phase1State(PhaseConfig config)
        {
            _config = config;
        }

        public void Enter(AIComponent ai)
        {
            _ai = ai;
            _behaviorTree = BuildBehaviorTree();
        }

        public void Update()
        {
            _behaviorTree?.Evaluate();
        }

        public void Exit()
        {
            _behaviorTree = null;
        }

        private Node BuildBehaviorTree()
        {
            return Fallback(
                // Highest priority: Transition to Phase 2
                Sequence(
                    new ConditionNode(() => _ai.NPC.life < _ai.NPC.lifeMax * 0.5f),
                    new ActionNode(() =>
                    {
                        _ai.ChangeState(new Phase2State()); // Placeholder for now
                        return NodeState.Success;
                    })
                ),

                // High priority: Chase player if too far
                Sequence(
                    new ConditionNode(() => _ai.Target != null && _ai.NPC.Distance(_ai.Target.Center) > 650f),
                    BT.SetChase(_ai, () => _ai.Target.Center, 320f)
                ),

                // Summon logic
                Sequence(
                    new ConditionNode(() => _ai.ShouldSummon),
                    BT.SetSpawnNpc(_ai, _config.Summon.NpcId, () => _ai.Target.Center + new Vector2(0, 1000)),
                    new ActionNode(() => { _ai.ShouldSummon = false; return NodeState.Success; })
                ),

                // Dash logic
                Sequence(
                    new ConditionNode(() => _ai.ShouldDash),
                    // Charge up
                    BT.SetChase(_ai, () => _ai.NPC.Center + (_ai.NPC.Center - _ai.Target.Center).SafeNormalize(Vector2.UnitX) * 100, 0),
                    Wait(_config.Dash.ChargeTime),
                    // Dash
                    BT.SetChase(_ai, () => _ai.Target.Center, 0), // This should be a high-speed chase
                    new ActionNode(() => { _ai.ResetDashTrigger(); return NodeState.Success; })
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
                    var targetPos = _ai.Target.Center + new Vector2(300 * _patrolDirection, -300);
                    _ai.Controller.GetComponent<IMovementComponent>().SetIntent(new ChaseIntent(targetPos, 50f));

                    // Check for turnaround
                    if (Math.Abs(_ai.NPC.Center.X - targetPos.X) < 100f)
                    {
                        _patrolDirection *= -1; // Reverse direction

                        // Fire projectile on turnaround
                        var attackComponent = _ai.Controller.GetComponent<IAttackComponent>();
                        if (attackComponent.IsReady())
                        {
                            attackComponent.SetIntent(new ShootProjectileIntent(_ai.Target.Center, _config.Attacks.BasicShot));
                        }

                        // 50% chance to dash is now handled by damage accumulation
                    }
                    return NodeState.Success;
                })
            );
        }
    }
}