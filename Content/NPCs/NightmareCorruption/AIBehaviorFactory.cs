using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    public class AIBehaviorFactory
    {
        private readonly BossConfig _bossConfig;
        private readonly Dictionary<string, Func<int, EcsWorld, NodeStatus>> _actionLookups;

        public AIBehaviorFactory(BossConfig bossConfig)
        {
            _bossConfig = bossConfig;
            _actionLookups = new Dictionary<string, Func<int, EcsWorld, NodeStatus>>
            {
                { "FindAndTargetPlayer", FindAndTargetPlayer },
                { "MoveToPlayer", MoveToPlayer },
                { "TryBasicAttack", TryBasicAttack }
            };
        }

        public Node CreateBehaviorTree(string name)
        {
            // For now, we assume the name corresponds to a phase in the config.
            // A more robust solution might involve a dictionary of trees in the config.
            if (name == "NightmareCorruption_Phase1" && _bossConfig.Phase1.BehaviorTree != null)
            {
                return ParseNode(_bossConfig.Phase1.BehaviorTree);
            }
            throw new ArgumentException($"Behavior tree '{name}' not found in config or factory.");
        }

        private Node ParseNode(BehaviorTreeConfig nodeConfig)
        {
            if (nodeConfig == null) return null;

            Node node = null;
            switch (nodeConfig.Type)
            {
                case "Selector":
                    node = new Selector();
                    break;
                case "Sequence":
                    node = new Sequence();
                    break;
                case "Action":
                    if (!string.IsNullOrEmpty(nodeConfig.Name) && _actionLookups.TryGetValue(nodeConfig.Name, out var action))
                    {
                        node = new ActionNode(action);
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown action name: {nodeConfig.Name}");
                    }
                    break;
                default:
                    throw new ArgumentException($"Unknown node type: {nodeConfig.Type}");
            }

            if (nodeConfig.Children != null)
            {
                foreach (var childConfig in nodeConfig.Children)
                {
                    var childNode = ParseNode(childConfig);
                    if (childNode != null && node is CompositeNode compositeNode)
                    {
                        compositeNode.Add(childNode);
                    }
                }
            }

            return node;
        }

        // --- Action Methods ---

        private NodeStatus FindAndTargetPlayer(int entityId, EcsWorld world)
        {
            var blackboard = world.GetComponent<AIBlackboardComponent>(entityId);
            if (blackboard == null)
            {
                ModContent.GetInstance<AshenVoid>().Logger.Warn($"Entity {entityId} is missing AIBlackboardComponent. Entity was not built correctly.");
                return NodeStatus.Failure;
            }

            Player target = null;
            float minDistance = float.MaxValue;
            var statSheet = world.GetComponent<StatSheetComponent>(entityId);
            if (statSheet == null) return NodeStatus.Failure;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active && !p.dead)
                {
                    float dist = statSheet.Npc.Distance(p.Center);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        target = p;
                    }
                }
            }

            if (target != null)
            {
                blackboard.Set(BlackboardKeys.Target, target);
                return NodeStatus.Success;
            }
            return NodeStatus.Failure;
        }

        private NodeStatus MoveToPlayer(int entityId, EcsWorld world)
        {
            var blackboard = world.GetComponent<AIBlackboardComponent>(entityId);
            var movement = world.GetComponent<MovementComponent>(entityId);
            var target = blackboard?.Get<Player>(BlackboardKeys.Target);

            if (movement == null || target == null) return NodeStatus.Failure;

            float distance = Vector2.Distance(movement.Npc.Center, target.Center);
            if (distance > _bossConfig.Phase1.Movement.ChaseStopDistance)
            {
                movement.TargetPosition = target.Center;
                return NodeStatus.Running;
            }

            return NodeStatus.Success;
        }

        private NodeStatus TryBasicAttack(int entityId, EcsWorld world)
        {
            const string attackName = "BasicShot";
            var attack = world.GetComponent<AttackComponent>(entityId);
            if (attack == null || !attack.CanAttack(attackName)) return NodeStatus.Failure;

            attack.UseAttack(attackName, _bossConfig.Phase1.Attacks.BasicShot.Cooldown);
            EcsSystem.Instance.EventBus.Publish(new AttackPerformedNetworkEvent { EntityId = entityId, AttackName = attackName });
            return NodeStatus.Success;
        }
    }
}