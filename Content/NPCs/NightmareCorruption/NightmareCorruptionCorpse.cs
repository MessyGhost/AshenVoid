using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    public class NightmareCorruptionCorpse : ModNPC
    {
        // Constants for better readability and maintenance.
        private const int FadeInTime = 30; // Time in ticks for the corpse to become fully visible
        private const int MinAlpha = 100;
        private const int MaxTimeLeft = 600;
        private const int DustSpawnChance = 10; // 1 in 10 chance per tick
        private const float Gravity = 0.5f;

        // Using ai[0] as a timer for synchronized dust effects in multiplayer.
        private ref float DustTimer => ref NPC.ai[0];

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
            // Fade-in logic
            if (NPC.alpha > MinAlpha)
            {
                NPC.alpha -= (255 - MinAlpha) / FadeInTime;
                if (NPC.alpha < MinAlpha)
                {
                    NPC.alpha = MinAlpha;
                }
            }

            NPC.velocity.Y = Gravity;

            // Ensure timeLeft doesn't exceed the max value.
            if (NPC.timeLeft > MaxTimeLeft)
            {
                NPC.timeLeft = MaxTimeLeft;
            }

            // Synchronized dust effect logic.
            // The timer runs on the server and syncs via npc.ai.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                DustTimer++;
                if (DustTimer >= DustSpawnChance)
                {
                    DustTimer = 0;
                    NPC.netUpdate = true; // Sync the timer reset and trigger dust on clients.
                }
            }

            // Clients spawn dust when the timer resets.
            if (DustTimer == 0)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Corruption, 0f, -1f, 0, default, 1.5f);
            }
        }
    }
}