using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace AshenVoid.Content.NPCs
{
    internal class NPCUtils
    {
        public static void ForceSyncNPC(int npcId)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcId);
            }
        }
    }
}
