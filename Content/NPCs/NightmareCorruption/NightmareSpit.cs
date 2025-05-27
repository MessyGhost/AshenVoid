using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    public class NightmareSpit : ModNPC
    {
        public override string Texture => "Terraria/Images/NPC_" + NPCID.VileSpit;
        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.VileSpit);
            NPC.damage = 32;
        }
    }
}
