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