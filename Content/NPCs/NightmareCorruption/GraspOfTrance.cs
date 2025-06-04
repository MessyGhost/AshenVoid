using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    public class GraspOfTrance : ModNPC
    {
        private Player Target => Main.player[(int)NPC.ai[0]];
        private NPC Boss => Main.npc[(int)NPC.ai[1]];

        public override void SetDefaults()
        {
            NPC.width = 40;
            NPC.height = 40;
            NPC.lifeMax = 500;
            NPC.damage = 30;
            NPC.defense = 10;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
        }

        public override void AI()
        {
            // 如果Boss或目标不存在，消失
            if (Target == null || !Target.active || Boss == null || !Boss.active)
            {
                NPC.active = false;
                return;
            }

            // 行为：上冲 -> 拐弯下落 -> 循环
            if (NPC.ai[2] == 0) // 上冲阶段
            {
                NPC.velocity = new Vector2(0, -10f);
                if (NPC.Center.Y <= Target.Center.Y + 160)
                {
                    NPC.ai[2] = 1; // 进入下落阶段
                }
            }
            else if (NPC.ai[2] == 1) // 下落阶段
            {
                NPC.velocity = new Vector2(0, 10f);

                // 如果距离玩家大于1000像素，则重新传送
                if (Vector2.Distance(NPC.Center, Target.Center) > 1000)
                {
                    NPC.Center = new Vector2(Target.Center.X, Target.Center.Y + 1000);
                    NPC.ai[2] = 0; // 重新上冲
                }
            }
        }
    }
}