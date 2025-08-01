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
        public IEnumerable<Type> RequiredComponents => new[] { typeof(AttackComponent) };
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public void Update(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var attack = world.GetComponent<AttackComponent>(entityId);
            attack?.UpdateCooldowns((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public AttackSystem()
        {
            // This subscription should ideally be in Load() or a similar setup method,
            // but for now, we'll keep it here.
            // A better approach would be to have the EcsSystem manage subscriptions.
            EcsSystem.Instance.EventBus.Subscribe<AttackPerformedNetworkEvent>(HandleAttack);
        }

        private void HandleAttack(AttackPerformedNetworkEvent e)
        {
            // Critical fix: Ensure projectile creation only happens on the server.
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            var world = EcsSystem.Instance.World;
            var statSheet = world.GetComponent<StatSheetComponent>(e.EntityId);
            var movement = world.GetComponent<MovementComponent>(e.EntityId);
            var blackboard = world.GetComponent<AIBlackboardComponent>(e.EntityId);

            if (statSheet == null || movement == null || blackboard == null) return;

            var target = blackboard.Get<Player>(BlackboardKeys.Target);
            if (target == null) return;

            if (e.AttackName == "BasicShot")
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