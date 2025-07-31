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
    /// It requires an AIComponent to be injected to access the various components.
    /// </summary>
    public static class BT
    {
        public static Node SetIdle(AIComponent ai)
        {
            return new ActionNode(() =>
            {
                ai.Controller.GetComponent<IMovementComponent>().SetIntent(new IdleIntent());
                return NodeState.Success;
            });
        }

        public static Node SetChase(AIComponent ai, Func<Vector2> targetPosition, float stopDistance = 100f)
        {
            return new ActionNode(() =>
            {
                ai.Controller.GetComponent<IMovementComponent>().SetIntent(new ChaseIntent(targetPosition(), stopDistance));
                return NodeState.Success;
            });
        }

        public static Node SetOrbit(AIComponent ai, Func<Vector2> center, float radius, int direction = 1)
        {
            return new ActionNode(() =>
            {
                ai.Controller.GetComponent<IMovementComponent>().SetIntent(new OrbitIntent(center(), radius, direction));
                return NodeState.Success;
            });
        }

        public static Node SetFlee(AIComponent ai, Func<Vector2> fleeFromPosition)
        {
            return new ActionNode(() =>
            {
                ai.Controller.GetComponent<IMovementComponent>().SetIntent(new FleeIntent(fleeFromPosition()));
                return NodeState.Success;
            });
        }

        // Example for a condition node
        public static Node IsPlayerInRange(AIComponent ai, float distance)
        {
            return new ConditionNode(() =>
            {
                if (ai.Target == null || !ai.Target.active) return false;
                return Vector2.Distance(ai.Target.Center, ai.NPC.Center) < distance;
            });
        }

        public static Node SetShootProjectile(AIComponent ai, Func<Vector2> target, ProjectileAttack stats)
        {
            return new ActionNode(() =>
            {
                var attackComponent = ai.Controller.GetComponent<IAttackComponent>();
                if (attackComponent == null || !attackComponent.IsReady())
                    return NodeState.Failure;

                attackComponent.SetIntent(new ShootProjectileIntent(target(), stats));
                return NodeState.Success;
            });
        }

        public static Node IsAttackReady(AIComponent ai)
        {
            return new ConditionNode(() =>
            {
                var attackComponent = ai.Controller.GetComponent<IAttackComponent>();
                return attackComponent != null && attackComponent.IsReady();
            });
        }

        public static Node IsAttacking(AIComponent ai)
        {
            return new ConditionNode(() =>
            {
                var attackComponent = ai.Controller.GetComponent<IAttackComponent>();
                return attackComponent != null && attackComponent.IsAttacking();
            });
        }
    }
}