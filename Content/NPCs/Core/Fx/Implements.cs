using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Core.Processor
{
    /// <summary>
    /// 基础的帧序列动画处理器
    /// </summary>
    public class Framer : Processor
    {
        public override void OnFindFrame(NPC npc)
        {
            // 帧动画逻辑
            npc.frameCounter++;
            if (npc.frameCounter >= 10)
            {
                npc.frameCounter = 0;
                npc.frame.Y = (npc.frame.Y + npc.height) % (Main.npcFrameCount[npc.type] * npc.height);
            }
        }
    }

    /// <summary>
    /// transform处理器
    /// </summary>
    namespace AshenVoid.Core.FX
    {
        public class Transformer : Processor
        {
            public Vector2 PositionOffset = Vector2.Zero;
            public float RotationOffset = 0f;

            public override void PostAI(NPC npc)
            {
                npc.position += PositionOffset;
                npc.rotation += RotationOffset;
            }
        }
    }

    /// <summary>
    /// 倾斜效果（基于速度）
    /// </summary>
    public class Tilter : Processor
    {
        // 改为使用函数获取最新速度
        private readonly Func<Vector2> _getVelocity;

        public Tilter(Func<Vector2> getVelocity)
        {
            _getVelocity = getVelocity;
        }

        public override void OnFindFrame(NPC npc)
        {
            Vector2 velocity = _getVelocity(); // 获取实时速度
            float maxTiltAngle = MathHelper.ToRadians(30);
            float tiltFactor = 0.001f;
            float smoothingFactor = 0.1f;

            if (Math.Abs(velocity.X) > 0.1f) // 添加阈值检测
            {
                float tiltDirection = Math.Sign(velocity.X);
                float tiltMagnitude = Math.Min(Math.Abs(velocity.X) * tiltFactor, 1f);
                float targetRotation = tiltDirection * MathHelper.Lerp(0, maxTiltAngle, tiltMagnitude);

                npc.rotation = MathHelper.Lerp(npc.rotation, targetRotation, smoothingFactor);
            }
            else
            {
                npc.rotation = MathHelper.Lerp(npc.rotation, 0f, smoothingFactor);
            }

            npc.spriteDirection = velocity.X > 0 ? 1 : -1;
        }
    }
}