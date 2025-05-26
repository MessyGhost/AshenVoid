using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.CorruptedLord
{
    public class CorruptedLordPhantom : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = Main.npcFrameCount[ModContent.NPCType<CorruptedLord>()];
        }

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 100;
            NPC.lifeMax = 9999;
            NPC.damage = 50;
            NPC.defense = 30;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.alpha = 80;
            NPC.value = 0f;
        }

        public override void AI()
        {
            // 永久虚影AI (ai[0] == 1)
            if (NPC.ai[0] == 1)
            {
                if (NPC.target < 0 || NPC.target >= 255 || !Main.player[NPC.target].active)
                {
                    NPC.TargetClosest();
                    if (NPC.target < 0 || NPC.target >= 255 || !Main.player[NPC.target].active)
                    {
                        NPC.active = false;
                        return;
                    }
                }

                Player player = Main.player[NPC.target];

                // 围绕玩家旋转
                float rotationSpeed = 0.02f;
                float distance = NPC.ai[2];
                NPC.ai[1] += rotationSpeed;

                Vector2 targetPos = player.Center + new Vector2(0, distance).RotatedBy(NPC.ai[1]);
                Vector2 direction = targetPos - NPC.Center;
                float speed = MathHelper.Clamp(direction.Length() * 0.1f, 5f, 20f);
                direction = direction.SafeNormalize(Vector2.Zero);

                NPC.velocity = direction * speed;
                NPC.rotation = 0f; // 禁止旋转
            }
        }

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