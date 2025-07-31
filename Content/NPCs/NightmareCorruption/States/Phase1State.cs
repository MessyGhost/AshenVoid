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
    public class Phase1State : IState
    {
        private readonly PhaseConfig _config;
        private Node _behaviorTree;
        private int _patrolDirection = 1;

        // Context
        private ComponentController _controller;
        private NPC _npc;
        private Player _target;
        private AIStateComponent _aiState;

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
            // Update context
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
                // Highest priority: Transition to Phase 2
                new SequenceNode(
                    new ConditionNode(() => _npc.life < _npc.lifeMax * _config.PhaseTransitionHealth),
                    new ActionNode(() =>
                    {
                        _aiState.ChangeState(new Phase2State(_config)); // Pass config to next phase
                        return NodeState.Success;
                    })
                ),

                // Summon logic
                new SequenceNode(
                    new ConditionNode(() => _aiState.ShouldSummon),
                    AIBehaviorFactory.SetSpawnNpc(_controller, _config.Summon.NpcId, () => safeTargetCenter() + new Vector2(0, 1000), _config.Summon.Count, _config.Summon.Cooldown),
                    new ActionNode(() => { _aiState.ShouldSummon = false; return NodeState.Success; })
                ),

                // Dash logic
                BuildDashBehavior(safeTargetCenter),

                // Default patrol and attack logic
                BuildPatrolBehavior(safeTargetCenter)
            );
        }

        private Node BuildDashBehavior(Func<Vector2> target)
        {
            return new SequenceNode(
                new ConditionNode(() => _aiState.ShouldDash),
                // Charge up phase
                new ActionNode(() =>
                {
                    // Move back slightly to telegraph the dash
                    var chargeDirection = (_npc.Center - target()).SafeNormalize(Vector2.UnitX);
                    _controller.GetComponent<IMovementComponent>().SetIntent(new Core.ECS.Intents.ChaseIntent(_npc.Center + chargeDirection * 150f, 0f));
                    return NodeState.Success;
                }),
                new WaitNode(_config.Dash.ChargeTime),
                // Dash action
                new ActionNode(() =>
                {
                    // Set a high-speed chase intent
                    _controller.GetComponent<IMovementComponent>().SetIntent(new Core.ECS.Intents.ChaseIntent(target(), 0f, 25f)); // Using high speed override
                    return NodeState.Success;
                }),
                new WaitNode(0.5f), // Duration of the dash
                new ActionNode(() =>
                {
                    _aiState.ResetDashTrigger();
                    return NodeState.Success;
                })
            );
        }

        private Node BuildPatrolBehavior(Func<Vector2> target)
        {
            return new SequenceNode(
                new ActionNode(() =>
                {
                    if (_target == null || !_target.active)
                    {
                        AIBehaviorFactory.SetIdle(_controller);
                        return NodeState.Failure;
                    }

                    var patrolTargetPosition = target() + new Vector2(400 * _patrolDirection, -300);
                    AIBehaviorFactory.SetChase(_controller, () => patrolTargetPosition, 80f);

                    // Check for turnaround and shoot
                    if (Vector2.Distance(_npc.Center, patrolTargetPosition) < 100f)
                    {
                        _patrolDirection *= -1;
                        AIBehaviorFactory.SetShootProjectile(_controller, target, _config.Attacks.BasicShot);
                    }
                    return NodeState.Success;
                })
            );
        }
    }
}