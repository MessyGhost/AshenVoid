using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using AshenVoid.Core.ECS.AI;

namespace AshenVoid.Core.ECS.Systems
{
    public class AttackSystem : IComponentSystem
    {
        public IEnumerable<Type> RequiredComponents => new[] { typeof(AttackComponent), typeof(StatSheetComponent) };
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public void Update(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var attack = world.GetComponent<AttackComponent>(entityId);
            attack?.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public AttackSystem()
        {
            // It's better to subscribe in a Load method or similar, but for simplicity...
            // A better approach would be for the SystemManager to pass the event bus to each system.
            // For now, this will work.
            EcsSystem.Instance.EventBus.Subscribe<AttackPerformedNetworkEvent>(HandleAttack);
        }

        private void HandleAttack(AttackPerformedNetworkEvent e)
        {
            var world = EcsSystem.Instance.World;
            var statSheet = world.GetComponent<StatSheetComponent>(e.EntityId);
            var movement = world.GetComponent<MovementComponent>(e.EntityId);
            var blackboard = world.GetComponent<AIBlackboardComponent>(e.EntityId);

            if (statSheet == null || movement == null || blackboard == null) return;

            var target = blackboard.Get<Player>(BlackboardKeys.Target);
            if (target == null) return;

            if (e.AttackId == 0) // Basic Shot
            {
                var attackConfig = statSheet.Config.Phase1.Attacks.BasicShot;
                Vector2 direction = Vector2.Normalize(target.Center - movement.Npc.Center);
                Vector2 velocity = direction * attackConfig.Speed;

                Projectile.NewProjectile(new EntitySource_Hostile(movement.Npc), movement.Npc.Center, velocity,
                    attackConfig.ProjectileId, attackConfig.Damage, 0f, Main.myPlayer);
            }
        }
    }
}