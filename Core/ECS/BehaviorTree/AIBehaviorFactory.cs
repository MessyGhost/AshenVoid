using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.BehaviorTree
{
    public class AIBehaviorFactory
    {
        private readonly Dictionary<string, Func<Node>> _behaviorTrees = new();
        private readonly BossConfig _bossConfig;

        public AIBehaviorFactory(BossConfig bossConfig)
        {
            _bossConfig = bossConfig;
            RegisterBehaviorTrees();
        }

        private void RegisterBehaviorTrees()
        {
            _behaviorTrees["NightmareCorruption_Phase1"] = CreatePhase1Tree;
        }

        public Node CreateBehaviorTree(string name)
        {
            if (_behaviorTrees.TryGetValue(name, out var factoryMethod))
            {
                return factoryMethod();
            }
            throw new ArgumentException($"Behavior tree '{name}' not found.");
        }

        private Node CreatePhase1Tree()
        {
            return new Selector()
                .Add(new Sequence()
                    .Add(new ActionNode(FindAndTargetPlayer))
                    .Add(new ActionNode(MoveToPlayer))
                    .Add(new ActionNode(TryBasicAttack))
                );
        }

        private NodeStatus FindAndTargetPlayer(int entityId, EcsWorld world)
        {
            var blackboard = world.GetComponent<AIBlackboardComponent>(entityId);
            if (blackboard == null)
            {
                // Create a blackboard if it doesn't exist
                blackboard = new AIBlackboardComponent();
                world.AddComponent(entityId, blackboard);
            }

            Player target = null;
            float minDistance = float.MaxValue;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active && !p.dead)
                {
                    var npc = world.GetComponent<MovementComponent>(entityId)?.Npc;
                    if (npc == null) return NodeStatus.Failure;

                    float dist = npc.Distance(p.Center);
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
            var attack = world.GetComponent<AttackComponent>(entityId);
            if (attack == null || !attack.CanAttack(0)) return NodeStatus.Failure;

            attack.UseAttack(0, _bossConfig.Phase1.Attacks.BasicShot.Cooldown);
            EcsSystem.Instance.EventBus.Publish(new AttackPerformedNetworkEvent { EntityId = entityId, AttackId = 0 });
            return NodeStatus.Success;
        }
    }
}