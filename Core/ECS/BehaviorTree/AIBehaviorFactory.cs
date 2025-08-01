using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.Intents;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.BehaviorTree
{
    public class AIBehaviorFactory
    {
        private readonly Blackboard _blackboard;

        public AIBehaviorFactory(Blackboard blackboard)
        {
            _blackboard = blackboard;
        }

        public Node CreateBehaviorTree(string treeName)
        {
            Node tree;
            switch (treeName)
            {
                case "NightmareCorruption_Phase1":
                    tree = BuildPhase1Tree();
                    break;
                // Add other phases or bosses here
                // case "NightmareCorruption_Phase2":
                //     tree = BuildPhase2Tree();
                //     break;
                default:
                    throw new ArgumentException($"No behavior tree found with the name: {treeName}");
            }
            
            // IMPORTANT: Set the blackboard for the entire tree
            tree.SetBlackboard(_blackboard);
            return tree;
        }

        private Node BuildPhase1Tree()
        {
            var config = _blackboard.Get<BossConfig>("BossConfig").Phase1;

            return new FallbackNode(
                // Dash logic
                BuildDashBehavior(config.Dash),
                // Default patrol and attack logic
                BuildPatrolBehavior(config.Attacks.BasicShot)
            );
        }

        private Node BuildDashBehavior(DashStats dashConfig)
        {
            string damageTakenKey = "DamageTakenSinceLastDash";

            return new SequenceNode(
                new ConditionNode(() => _blackboard.Get<float>(damageTakenKey) >= dashConfig.DamageThreshold),
                // Charge up phase
                new ActionNode(() =>
                {
                    var npc = _blackboard.Get<NPC>(BlackboardKeys.NPC);
                    var target = _blackboard.Get<Player>(BlackboardKeys.Target);
                    var chargeDirection = (npc.Center - target.Center).SafeNormalize(Vector2.UnitX);
                    _blackboard.Set(BlackboardKeys.MovementIntent, new ChaseIntent(npc.Center + chargeDirection * 150f, 0f));
                    return NodeState.Success;
                }),
                new WaitNode(dashConfig.ChargeTime),
                // Dash action
                new SetMovementIntentNode(new ChaseIntent(_blackboard.Get<Player>(BlackboardKeys.Target).Center, 0f, 25f)),
                new WaitNode(0.5f), // Duration of the dash
                // Reset
                new SetBlackboardValueNode<float>(damageTakenKey, 0f),
                new SetMovementIntentNode(new IdleIntent())
            );
        }

        private Node BuildPatrolBehavior(ProjectileAttack attackStats)
        {
            string patrolDirKey = "PatrolDirection";

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

                    if (!_blackboard.Has(patrolDirKey))
                    {
                        _blackboard.Set(patrolDirKey, 1);
                    }
                    int patrolDirection = _blackboard.Get<int>(patrolDirKey);

                    var patrolTargetPosition = target.Center + new Vector2(400 * patrolDirection, -300);
                    _blackboard.Set(BlackboardKeys.MovementIntent, new ChaseIntent(patrolTargetPosition, 80f));

                    if (Vector2.Distance(npc.Center, patrolTargetPosition) < 100f)
                    {
                        _blackboard.Set(patrolDirKey, patrolDirection * -1);
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