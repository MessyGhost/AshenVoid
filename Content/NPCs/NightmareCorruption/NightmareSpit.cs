using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
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
            NPC.aiStyle = -1;
        }

        public override void AI()
        {
            if(Main.rand.NextBool(3))
            {
                Dust.NewDust(NPC.Center, 0, 0, DustID.CorruptGibs);
            }
            if(NPC.collideX || NPC.collideY)
            {
                for(int i = 0; i < 3; ++i)
                {
                    Dust.NewDust(NPC.Center, 0, 0, DustID.CorruptGibs);
                }
                NPC.active = false;
                NPC.netUpdate = true;
            }
        }
    }
}
