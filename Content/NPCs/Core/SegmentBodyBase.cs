using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs
{
    public abstract class SegmentBodyBase : ModNPC
    {
        protected NPC Head => Main.npc[NPC.realLife];
        protected NPC Following => Main.npc[(int)NPC.ai[0]];

        public override void AI()
        {
            if (Head == null || !Head.active)
            {
                NPC.active = false;
                return;
            }

            if (Following != null && Following.active)
            {
                Vector2 toFollowing = Following.Center - NPC.Center;
                float dist = toFollowing.Length();
                Vector2 direction = toFollowing.SafeNormalize(Vector2.Zero);

                NPC.Center += direction * Math.Max(0.0f, dist - (Following.width + NPC.width) / 2);
                NPC.rotation = (float)System.Math.Atan2(direction.Y, direction.X);
            }
        }
    }
}