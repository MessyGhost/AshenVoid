using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.Intents;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace AshenVoid.Core.ECS.BehaviorTree
{
    public class AIBehaviorFactory
    {
        private readonly ServiceLocator _services;

        public AIBehaviorFactory(ServiceLocator services)
        {
            _services = services;
        }

        public Node CreateBehaviorTree(string treeName)
        {
            switch (treeName)
            {
                case "NightmareCorruption_Phase1":
                    return BuildPhase1Tree();
                default:
                    throw new ArgumentException($"No behavior tree found with the name: {treeName}");
            }
        }

        private Node BuildPhase1Tree()
        {
            var config = _services.Get<BossConfig>();

            return new FallbackNode(
                BuildChaseBehavior(config.Phase1.ChaseDistanceThreshold),
                BuildSummonBehavior(),
                BuildDamageDashBehavior(config.Phase1.Dash),
                BuildPatrolBehavior(config.Phase1)
            );
        }

        private Node BuildChaseBehavior(float chaseDistance)
        {
            return new SequenceNode(
                new ConditionNode(bb => 
                {
                    var npc = bb.Get<NPC>(BlackboardKeys.NPC);
                    var target = bb.Get<Player>(BlackboardKeys.Target);
                    return target != null && npc.Distance(target.Center) > chaseDistance;
                }),
                new ActionNode(bb =>
                {
                    var target = bb.Get<Player>(BlackboardKeys.Target);
                    var movementStats = _services.Get<BossConfig>().Phase1.Movement;
                    bb.Set(BlackboardKeys.MovementIntent, new ChaseIntent(target.Center, 0f, movementStats.MaxSpeed * 2.5f));
                    return NodeState.Running;
                })
            );
        }

        private Node BuildSummonBehavior()
        {
            return new SequenceNode(
                new ConditionNode(bb => bb.Get<bool>("ShouldSummonGrasp")),
                new ActionNode(bb => 
                {
                    // TODO: Implement the actual summoning logic (e.g., via a SummonIntent)
                    // For now, just log it and reset the flag.
                    ModContent.GetInstance<AshenVoid>().Logger.Info("Summoning Grasp of Trance");
                    bb.Set("ShouldSummonGrasp", false);
                    return NodeState.Success;
                })
            );
        }

        private Node BuildDamageDashBehavior(DashStats dashConfig)
        {
            string damageTakenKey = "DamageTakenSinceLastDash";

            return new SequenceNode(
                new ConditionNode(bb => bb.Get<float>(damageTakenKey) >= dashConfig.DamageThreshold),
                // Charge up
                new ActionNode(bb =>
                {
                    var npc = bb.Get<NPC>(BlackboardKeys.NPC);
                    var target = bb.Get<Player>(BlackboardKeys.Target);
                    var chargeDirection = (npc.Center - target.Center).SafeNormalize(Vector2.UnitX);
                    bb.Set(BlackboardKeys.MovementIntent, new ChaseIntent(npc.Center + chargeDirection * 150f, 0f));
                    // TODO: Trigger charging VFX
                    return NodeState.Success;
                }),
                new WaitNode(dashConfig.ChargeTime),
                // Dash
                new ActionNode(bb =>
                {
                    var target = bb.Get<Player>(BlackboardKeys.Target);
                    bb.Set(BlackboardKeys.MovementIntent, new ChaseIntent(target.Center, 0f, dashConfig.DashSpeed));
                    // TODO: Spawn corruption monsters on self
                    return NodeState.Success;
                }),
                new WaitNode(0.5f), // Dash duration
                // Reset
                new ActionNode(bb =>
                {
                    bb.Set(damageTakenKey, 0f);
                    bb.Set(BlackboardKeys.MovementIntent, new IdleIntent());
                    return NodeState.Success;
                })
            );
        }

        private Node BuildPatrolBehavior(PhaseConfig phaseConfig)
        {
            string patrolDirKey = "PatrolDirection";
            string patrolTimerKey = "PatrolTimer";
            float patrolTurnTime = 3f;

            return new SequenceNode(
                new ActionNode(bb =>
                {
                    var npc = bb.Get<NPC>(BlackboardKeys.NPC);
                    var target = bb.Get<Player>(BlackboardKeys.Target);
                    
                    if (target == null || !target.active)
                    {
                        bb.Set(BlackboardKeys.MovementIntent, new IdleIntent());
                        return NodeState.Failure;
                    }

                    if (!bb.Has(patrolDirKey))
                    {
                        bb.Set(patrolDirKey, npc.Center.X < target.Center.X ? 1 : -1);
                        bb.Set(patrolTimerKey, 0f);
                    }

                    float timer = bb.Get<float>(patrolTimerKey);
                    timer += (float)bb.Get<GameTime>(BlackboardKeys.GameTime).ElapsedGameTime.TotalSeconds;

                    var patrolTargetPosition = target.Center + new Vector2(350 * bb.Get<int>(patrolDirKey), -300);
                    bb.Set(BlackboardKeys.MovementIntent, new ChaseIntent(patrolTargetPosition, 80f, phaseConfig.Movement.MaxSpeed));

                    if (timer > patrolTurnTime || npc.Distance(patrolTargetPosition) < 100f)
                    {
                        bb.Set(patrolDirKey, bb.Get<int>(patrolDirKey) * -1);
                        bb.Set(patrolTimerKey, 0f);
                        bb.Set(BlackboardKeys.AttackIntent, new ShootProjectileIntent(target.Center, phaseConfig.Attacks.BasicShot));

                        if (Main.rand.NextBool())
                        {
                            bb.Set(BlackboardKeys.MovementIntent, new ChaseIntent(target.Center, 0f, phaseConfig.Movement.MaxSpeed * 2f));
                        }
                    }
                    else
                    {
                        bb.Set(patrolTimerKey, timer);
                    }

                    return NodeState.Success;
                })
            );
        }
    }
}