using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class ClientInterpolationSystem : EntityQuerySystem
    {
        public override IEnumerable<Type> RequiredComponents => new[] { typeof(MovementComponent) };
        public override SystemExecutionSide ExecutionSide => SystemExecutionSide.Client;

        public override void UpdateEntity(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var movement = world.GetComponent<MovementComponent>(entityId);
            if (movement == null || movement.NetPosition == Vector2.Zero) return;

            // 使用二阶动力学系统进行平滑插值
            // 将网络位置作为目标进行更新
            movement.Dynamics.Update((float)gameTime.ElapsedGameTime.TotalSeconds, movement.NetPosition);

            // 更新 NPC 的实际位置
            // Dynamics.Position 是中心点，需要转换为左上角坐标
            movement.Npc.position = movement.Dynamics.Position - movement.Npc.Size / 2f;
            movement.Npc.velocity = movement.Dynamics.Velocity; // 同时更新速度，使动画更自然
        }
    }
}