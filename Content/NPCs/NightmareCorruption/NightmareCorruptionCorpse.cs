using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    public class NightmareCorruptionCorpse : ModNPC
    {
        public override string Texture => "Terraria/Images/Gore_262";

        public override void SetDefaults()
        {
            NPC.width = 80;
            NPC.height = 80;
            NPC.lifeMax = 1;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.dontTakeDamage = true;
            NPC.immortal = true;
            NPC.aiStyle = -1;
            NPC.alpha = 255;
        }

        public override void AI()
        {
            NPC.alpha -= 5;
            if (NPC.alpha < 100) NPC.alpha = 100;

            NPC.velocity.Y = 0.5f;

            // 随时间消失
            if (NPC.timeLeft > 600) NPC.timeLeft = 600;

            // 粒子效果
            if (Main.rand.NextBool(10))
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Corruption, 0f, -1f, 0, default, 1.5f);
            }
        }
    }
}