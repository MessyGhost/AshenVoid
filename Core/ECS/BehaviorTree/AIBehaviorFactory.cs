using AshenVoid.Core.ECS.BehaviorTree;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.Intents;
using AshenVoid.Core.ECS.Interfaces;

namespace AshenVoid.Core.ECS.BehaviorTree
{
    /// <summary>
    /// A builder class for creating Behavior Tree nodes that interact with the ECS.
    /// It requires a ComponentController and context (NPC, Player) to be injected.
    /// </summary>
    public static class AIBehaviorFactory
    {
        public static Node SetIdle(ComponentController controller)
        {
            return new ActionNode(() =>
            {
                controller.GetComponent<IMovementComponent>().SetIntent(new IdleIntent());
                return NodeState.Success;
            });
        }

        public static Node SetChase(ComponentController controller, Func<Vector2> targetPosition, float stopDistance = 100f)
        {
            return new ActionNode(() =>
            {
                controller.GetComponent<IMovementComponent>().SetIntent(new ChaseIntent(targetPosition(), stopDistance));
                return NodeState.Success;
            });
        }

        public static Node SetOrbit(ComponentController controller, Func<Vector2> center, float radius, int direction = 1)
        {
            return new ActionNode(() =>
            {
                controller.GetComponent<IMovementComponent>().SetIntent(new OrbitIntent(center(), radius, direction));
                return NodeState.Success;
            });
        }

        public static Node SetFlee(ComponentController controller, Func<Vector2> fleeFromPosition)
        {
            return new ActionNode(() =>
            {
                controller.GetComponent<IMovementComponent>().SetIntent(new FleeIntent(fleeFromPosition()));
                return NodeState.Success;
            });
        }

        // Example for a condition node
        public static Node IsPlayerInRange(NPC npc, Player target, float distance)
        {
            return new ConditionNode(() =>
            {
                if (target == null || !target.active) return false;
                return Vector2.Distance(target.Center, npc.Center) < distance;
            });
        }

        public static Node SetShootProjectile(ComponentController controller, Func<Vector2> target, ProjectileAttack stats)
        {
            return new ActionNode(() =>
            {
                var attackComponent = controller.GetComponent<IAttackComponent>();
                if (attackComponent == null || !attackComponent.IsReady())
                    return NodeState.Failure;

                attackComponent.SetIntent(new ShootProjectileIntent(target(), stats));
                return NodeState.Success;
            });
        }

        public static Node IsAttackReady(ComponentController controller)
        {
            return new ConditionNode(() =>
            {
                var attackComponent = controller.GetComponent<IAttackComponent>();
                return attackComponent != null && attackComponent.IsReady();
            });
        }

        public static Node IsAttacking(ComponentController controller)
        {
            return new ConditionNode(() =>
            {
                var attackComponent = controller.GetComponent<IAttackComponent>();
                return attackComponent != null && attackComponent.IsAttacking();
            });
        }

        public static Node SetTeleport(ComponentController controller, Func<Vector2> targetPosition)
        {
            return new ActionNode(() =>
            {
                controller.GetComponent<IMovementComponent>().SetIntent(new TeleportIntent(targetPosition()));
                return NodeState.Success;
            });
        }

        public static Node SetSpawnNpc(ComponentController controller, int npcId, Func<Vector2> spawnPosition, int count = 1, float cooldown = 0f)
        {
            return new ActionNode(() =>
            {
                var attackComponent = controller.GetComponent<IAttackComponent>();
                if (attackComponent == null || !attackComponent.IsReady())
                    return NodeState.Failure;

                attackComponent.SetIntent(new SpawnNpcIntent(npcId, spawnPosition(), count, cooldown));
                return NodeState.Success;
            });
        }
    }
}