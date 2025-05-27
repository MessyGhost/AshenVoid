using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    public class NightmareCorruptionPhantom : ModNPC
    {
        public override string Texture => "AshenVoid/Content/NPCs/NightmareCorruption/NightmareOfCorruption";

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
    }
}