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
    public class AttackSystem : CachedComponentSystem
    {
        public override IEnumerable<Type> RequiredComponents => new[] { typeof(AttackComponent) };
        public override SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public override void UpdateEntity(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var attack = world.GetComponent<AttackComponent>(entityId);
            attack?.UpdateCooldowns((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public AttackSystem()
        {
            EcsSystem.Instance.EventBus.Subscribe<RequestAttackExecutionEvent>(HandleAttack);
        }

        private void HandleAttack(RequestAttackExecutionEvent e)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            var world = EcsSystem.Instance.World;
            var statSheet = world.GetComponent<StatSheetComponent>(e.EntityId);
            var movement = world.GetComponent<MovementComponent>(e.EntityId);
            var targetComponent = world.GetComponent<TargetComponent>(e.EntityId);

            if (statSheet == null || movement == null || targetComponent == null) return;

            var target = targetComponent.Target;
            if (target == null) return;

            if (e.AttackType == AttackType.BasicShot)
            {
                var attackConfig = statSheet.Config.Phase1.Attacks.BasicShot;
                Vector2 direction = Vector2.Normalize(target.Center - movement.Npc.Center);
                Vector2 velocity = direction * attackConfig.Speed;

                Projectile.NewProjectile(movement.Npc.GetSource_FromAI(), movement.Npc.Center, velocity,
                    attackConfig.ProjectileId, (int)attackConfig.Damage, 0f, Main.myPlayer);
            }
        }
    }
}