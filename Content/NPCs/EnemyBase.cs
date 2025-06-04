using System;
using AshenVoid.Core.BehaviorTree;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs
{
    public abstract class EnemyBase : ModNPC
    {
        // 目标玩家
        public Player TargetPlayer
        {
            get
            {
                if (NPC.target >= 0 && NPC.target < Main.maxPlayers)
                    return Main.player[NPC.target];
                return null;
            }
        }

        // 行为树
        protected Node BehaviorTree;

        public override void AI()
        {
            // 初始化行为树（每个NPC子类需重写）
            if (BehaviorTree == null)
                InitializeBehaviorTree();

            // 更新行为树
            BehaviorTree?.Evaluate();
        }

        // 子类需重写此方法以定义行为树
        protected abstract void InitializeBehaviorTree();


        // 确保有目标
        public void TargetIfRequired(bool faceTarget = false)
        {
            if (!NPC.HasValidTarget)
            {
                NPC.TargetClosest(faceTarget);
            }
        }

        // 播放声音
        public void PlaySound(SoundStyle sound)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(sound, NPC.Center);
            }
        }

        // 移动到目标位置
        protected void MoveToPosition(Vector2 targetPos, float maxSpeed = 28.0f, float maxAcceleration = 5.0f)
        {
            var toDest = targetPos - NPC.Center;
            var direction = toDest.SafeNormalize(Vector2.Zero);
            var dist = toDest.Length();

            var acc = direction * maxAcceleration;
            NPC.velocity += acc;
            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * Math.Min(NPC.velocity.Length(), maxSpeed);
        }

        // 动画帧更新
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter >= 10)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (Main.npcFrameCount[NPC.type] * frameHeight);
            }
        }
    }
}