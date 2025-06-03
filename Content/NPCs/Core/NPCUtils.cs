using Terraria;
using Terraria.ID;

namespace AshenVoid.Core
{
    public static class NPCUtils
    {
        public static void ForceSyncNPC(int npcId)
        {
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcId);
        }

        public static Player GetTargetPlayer(int target) =>
            target >= 0 && target < Main.maxPlayers ? Main.player[target] : null;
    }
}