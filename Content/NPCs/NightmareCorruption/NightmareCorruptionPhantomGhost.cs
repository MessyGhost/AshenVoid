using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    public class NightmareCorruptionPhantomGhost : NightmareCorruptionGhost
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.alpha = 255;
            Projectile.scale = 2.0f;
        }

        public override void AI()
        {
            Projectile.Center = Main.npc[(int)Projectile.ai[0]].Center;
            Projectile.alpha -= (255 - NightmareCorruptionPhantom.Alpha) / Life;
            Projectile.scale -= 1.0f / Life;
        }
    }
}
