using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

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

        public static void ForceSyncNPCWithAction(int npcId, Action action)
        {
            if(Main.netMode != NetmodeID.MultiplayerClient)
            {
                action();
            }

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcId);
            }
        }

        public static void TargetIfRequired(ModNPC npc, bool faceTarget = false)
        {
            if(!npc.NPC.HasValidTarget)
            {
                npc.NPC.TargetClosest(faceTarget);
            }
        }

        public static Player GetTargetPlayer(int target)
        {
            if (target >= 0 )
            {
                return Main.player[target];
            } else
            {
                return null;
            }
        }

        public static void PlaySound(ModNPC npc, SoundStyle sound)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(sound, npc.NPC.Center);
            }
        }
    }
}
