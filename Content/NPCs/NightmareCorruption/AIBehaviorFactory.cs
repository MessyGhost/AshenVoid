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
        private readonly Dictionary<AIActionType, Func<int, EcsWorld, NodeStatus>> _actionLookups;
        private readonly Dictionary<string, Node> _treeCache = new();

        public AIBehaviorFactory(BossConfig bossConfig)
        {
            _bossConfig = bossConfig;
            _actionLookups = new Dictionary<AIActionType, Func<int, EcsWorld, NodeStatus>>
            {
                { AIActionType.FindAndTargetPlayer, FindAndTargetPlayer },
                { AIActionType.MoveToPlayer, MoveToPlayer },
                { AIActionType.TryBasicAttack, TryBasicAttack }
            };
        }

        public Node CreateBehaviorTree(string name)
        {
            if (_treeCache.TryGetValue(name, out var cachedTree))
            {
                return cachedTree;
            }

            BehaviorTreeConfig config = null;
            if (name == "NightmareCorruption_Phase1" && _bossConfig.Phase1.BehaviorTree != null)
            {
                config = _bossConfig.Phase1.BehaviorTree;
            }

            if (config != null)
            {
                var newTree = ParseNode(config);
                _treeCache[name] = newTree;
                return newTree;
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
                    if (!string.IsNullOrEmpty(nodeConfig.Name) && Enum.TryParse(nodeConfig.Name, out AIActionType actionType) && _actionLookups.TryGetValue(actionType, out var action))
                    {
                        node = new ActionNode(action);
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown or invalid action name: {nodeConfig.Name}");
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
            var targetComponent = world.GetComponent<TargetComponent>(entityId);
            if (targetComponent == null) return NodeStatus.Failure;

            Player bestTarget = null;
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
                        bestTarget = p;
                    }
                }
            }

            if (bestTarget != null)
            {
                targetComponent.Target = bestTarget;
                return NodeStatus.Success;
            }
            return NodeStatus.Failure;
        }

        private NodeStatus MoveToPlayer(int entityId, EcsWorld world)
        {
            var targetComponent = world.GetComponent<TargetComponent>(entityId);
            var movement = world.GetComponent<MovementComponent>(entityId);
            var target = targetComponent?.Target;

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
            var attack = world.GetComponent<AttackComponent>(entityId);
            if (attack == null || !attack.CanAttack(AttackType.BasicShot)) return NodeStatus.Failure;

            attack.UseAttack(AttackType.BasicShot, _bossConfig.Phase1.Attacks.BasicShot.Cooldown);
            EcsSystem.Instance.EventBus.Publish(new RequestAttackExecutionEvent { EntityId = entityId, AttackType = AttackType.BasicShot });
            return NodeStatus.Success;
        }
    }
}