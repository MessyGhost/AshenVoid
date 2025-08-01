using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Intents;
using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase1State : IState
    {
        private static readonly string PatrolDirectionKey = "Phase1State_PatrolDirection";
        private static readonly string BehaviorTreeKey = "Phase1State_BehaviorTree";

        public void Enter(Blackboard blackboard)
        {
            blackboard.Set(PatrolDirectionKey, 1);
            // Build the behavior tree ONCE and store it in the blackboard.
            blackboard.Set(BehaviorTreeKey, BuildBehaviorTree(blackboard));
        }

        public Type Update(Blackboard blackboard)
        {
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
            var config = blackboard.Get<BossConfig>("BossConfig").Phase1;

            // Check for phase transition first, as it's the highest priority.
            if (npc.life < npc.lifeMax * config.PhaseTransitionHealth)
            {
                return typeof(Phase2State);
            }

            // Evaluate the behavior tree.
            var behaviorTree = blackboard.Get<Node>(BehaviorTreeKey);
            behaviorTree?.Evaluate();

            return null; // No transition requested by default
        }

        public void Exit(Blackboard blackboard)
        {
            // Clean up blackboard data
            blackboard.Remove(PatrolDirectionKey);
            blackboard.Remove(BehaviorTreeKey);
        }

        private Node BuildBehaviorTree(Blackboard blackboard)
        {
            var config = blackboard.Get<BossConfig>("BossConfig").Phase1;

            return new FallbackNode(
                // Summon logic
                new SequenceNode(
                    new ConditionNode(() => blackboard.Get<bool>("ShouldSummon")),
                    new ActionNode(() =>
                    {
                        Terraria.ModLoader.ModContent.GetInstance<AshenVoid>().Logger.Info("Summoning minions would happen here.");
                        return NodeState.Success;
                    }),
                    new ActionNode(() => { blackboard.Set("ShouldSummon", false); return NodeState.Success; })
                ),

                // Dash logic
                BuildDashBehavior(blackboard, config.Dash),

                // Default patrol and attack logic
                BuildPatrolBehavior(blackboard, config.Attacks.BasicShot)
            );
        }

        private Node BuildDashBehavior(Blackboard blackboard, DashStats dashConfig)
        {
            return new SequenceNode(
                new ConditionNode(() => blackboard.Get<float>("DamageTakenSinceLastDash") >= dashConfig.DamageThreshold),
                // Charge up phase
                new ActionNode(() =>
                {
                    var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
                    var target = blackboard.Get<Player>(BlackboardKeys.Target);
                    var chargeDirection = (npc.Center - target.Center).SafeNormalize(Vector2.UnitX);
                    blackboard.Set(BlackboardKeys.MovementIntent, new ChaseIntent(npc.Center + chargeDirection * 150f, 0f));
                    return NodeState.Success;
                }),
                new WaitNode(dashConfig.ChargeTime),
                // Dash action
                new ActionNode(() =>
                {
                    var target = blackboard.Get<Player>(BlackboardKeys.Target);
                    blackboard.Set(BlackboardKeys.MovementIntent, new ChaseIntent(target.Center, 0f, 25f));
                    return NodeState.Success;
                }),
                new WaitNode(0.5f), // Duration of the dash
                new ActionNode(() =>
                {
                    blackboard.Set("DamageTakenSinceLastDash", 0f);
                    blackboard.Set(BlackboardKeys.MovementIntent, new IdleIntent()); // End dash with an idle intent
                    return NodeState.Success;
                })
            );
        }

        private Node BuildPatrolBehavior(Blackboard blackboard, ProjectileAttack attackStats)
        {
            return new SequenceNode(
                new ActionNode(() =>
                {
                    var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
                    var target = blackboard.Get<Player>(BlackboardKeys.Target);
                    int patrolDirection = blackboard.Get<int>(PatrolDirectionKey);

                    if (target == null || !target.active)
                    {
                        blackboard.Set(BlackboardKeys.MovementIntent, new IdleIntent());
                        return NodeState.Failure;
                    }

                    var patrolTargetPosition = target.Center + new Vector2(400 * patrolDirection, -300);
                    blackboard.Set(BlackboardKeys.MovementIntent, new ChaseIntent(patrolTargetPosition, 80f));

                    if (Vector2.Distance(npc.Center, patrolTargetPosition) < 100f)
                    {
                        blackboard.Set(PatrolDirectionKey, patrolDirection * -1);
                        if (attackStats != null)
                        {
                            blackboard.Set(BlackboardKeys.AttackIntent, new ShootProjectileIntent(target.Center, attackStats));
                        }
                    }
                    return NodeState.Success;
                })
            );
        }
    }
}