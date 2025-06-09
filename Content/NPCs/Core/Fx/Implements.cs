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
        public Vector2 velocity;
        public Tilter(Vector2 v)
        {
            velocity = v;
        }
        public override void OnFindFrame(NPC npc)
        {
            // 使用低通滤波平滑旋转角度
            float maxTiltAngle = MathHelper.ToRadians(30);
            float tiltFactor = 0.001f;
            float smoothingFactor = 0.1f; // 平滑因子，值越大过渡越平滑

            if (velocity.X != 0)
            {
                float tiltDirection = Math.Sign(velocity.X);
                float tiltMagnitude = Math.Min(Math.Abs(velocity.X) * tiltFactor, 1f);
                float targetRotation = tiltDirection * MathHelper.Lerp(0, maxTiltAngle, tiltMagnitude);

                // 使用线性插值平滑过渡
                npc.rotation = MathHelper.Lerp(npc.rotation, targetRotation, smoothingFactor);
            }
            else
            {
                // 平滑回归到0度
                npc.rotation = MathHelper.Lerp(npc.rotation, 0f, smoothingFactor);
            }

            npc.spriteDirection = velocity.X > 0 ? 1 : -1;

        }
    }
}