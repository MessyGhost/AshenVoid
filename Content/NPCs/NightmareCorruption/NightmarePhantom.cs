using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    public class NightmarePhantom : ModNPC
    {
        private NPC Boss => Main.npc[(int)NPC.ai[0]];
        private int PhantomIndex => (int)NPC.ai[1];
        public override string Texture => "AshenVoid/Content/NPCs/NightmareCorruption/NightmareCorruption";

        public override void SetDefaults()
        {
            NPC.width = 60;
            NPC.height = 60;
            NPC.lifeMax = 1; // 无敌
            NPC.damage = 0;
            NPC.defense = 999;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.alpha = 150; // 半透明
            NPC.dontTakeDamage = true;
        }

        public override void AI()
        {
            // 如果Boss不存在，消失
            if (Boss == null || !Boss.active)
            {
                NPC.active = false;
                return;
            }

            // 跟随Boss位置
            if (NPC.Distance(Boss.Center) > 3000f)
            {
                NPC.Center = Boss.Center;
            }
        }
    }
}