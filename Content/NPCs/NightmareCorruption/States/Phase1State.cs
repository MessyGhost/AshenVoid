using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Intents;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase1State : IState
    {
        private PhaseConfig _config;
        private Node _behaviorTree;
        private int _patrolDirection = 1;
        private Blackboard _blackboard;

        public void Enter(Blackboard blackboard)
        {
            _blackboard = blackboard;
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
                // Highest priority: Transition to Phase 2
                new SequenceNode(
                    new ConditionNode(() => _blackboard.Get<NPC>(BlackboardKeys.NPC).life < _blackboard.Get<NPC>(BlackboardKeys.NPC).lifeMax * _config.PhaseTransitionHealth),
                    new ActionNode(() =>
                    {
                        _blackboard.Get<AIStateComponent>(BlackboardKeys.AIState).ChangeState<Phase2State>();
                        return NodeState.Success;
                    })
                ),

                // Summon logic
                new SequenceNode(
                    new ConditionNode(() => _blackboard.Get<bool>("ShouldSummon")),
                    new ActionNode(() =>
                    {
                        Terraria.ModLoader.ModContent.GetInstance<AshenVoid>().Logger.Info("Summoning minions would happen here.");
                        return NodeState.Success;
                    }),
                    new ActionNode(() => { _blackboard.Set("ShouldSummon", false); return NodeState.Success; })
                ),

                // Dash logic
                BuildDashBehavior(),

                // Default patrol and attack logic
                BuildPatrolBehavior()
            );
        }

        private Node BuildDashBehavior()
        {
            return new SequenceNode(
                new ConditionNode(() => _blackboard.Get<float>("DamageTakenSinceLastDash") >= _config.Dash.DamageThreshold),
                // Charge up phase
                new ActionNode(() =>
                {
                    var npc = _blackboard.Get<NPC>(BlackboardKeys.NPC);
                    var target = _blackboard.Get<Player>(BlackboardKeys.Target);

                    var chargeDirection = (npc.Center - target.Center).SafeNormalize(Vector2.UnitX);
                    _blackboard.Set(BlackboardKeys.MovementIntent, new ChaseIntent(npc.Center + chargeDirection * 150f, 0f));
                    return NodeState.Success;
                }),
                new WaitNode(_config.Dash.ChargeTime),
                // Dash action
                new ActionNode(() =>
                {
                    var target = _blackboard.Get<Player>(BlackboardKeys.Target);
                    _blackboard.Set(BlackboardKeys.MovementIntent, new ChaseIntent(target.Center, 0f, 25f));
                    return NodeState.Success;
                }),
                new WaitNode(0.5f), // Duration of the dash
                new ActionNode(() =>
                {
                    _blackboard.Set("DamageTakenSinceLastDash", 0f);
                    _blackboard.Set(BlackboardKeys.MovementIntent, new IdleIntent()); // End dash with an idle intent
                    return NodeState.Success;
                })
            );
        }

        private Node BuildPatrolBehavior()
        {
            return new SequenceNode(
                new ActionNode(() =>
                {
                    var npc = _blackboard.Get<NPC>(BlackboardKeys.NPC);
                    var target = _blackboard.Get<Player>(BlackboardKeys.Target);

                    if (target == null || !target.active)
                    {
                        _blackboard.Set(BlackboardKeys.MovementIntent, new IdleIntent());
                        return NodeState.Failure;
                    }

                    var patrolTargetPosition = target.Center + new Vector2(400 * _patrolDirection, -300);
                    _blackboard.Set(BlackboardKeys.MovementIntent, new ChaseIntent(patrolTargetPosition, 80f));

                    if (Vector2.Distance(npc.Center, patrolTargetPosition) < 100f)
                    {
                        _patrolDirection *= -1;
                        var attackStats = _config.Attacks.BasicShot;
                        if (attackStats != null)
                        {
                            _blackboard.Set(BlackboardKeys.AttackIntent, new ShootProjectileIntent(target.Center, attackStats));
                        }
                    }
                    return NodeState.Success;
                })
            );
        }
    }
}