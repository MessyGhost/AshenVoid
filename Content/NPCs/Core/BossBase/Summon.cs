using AshenVoid.Core.BehaviorTree;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using static AshenVoid.Core.BehaviorTree.NodeBuilder;

namespace AshenVoid.Content.NPCs
{
    public abstract partial class BossBase
    {
        #region 召唤节点
        // 召唤小怪
        protected NodeState SummonMinions(int minionType, int count)
        {
            if (TargetPlayer == null) return NodeState.Failure;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < count; i++)
                {
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, minionType);
                }
                hasSummonedMinions = true;
                return NodeState.Success;
            }
            return NodeState.Failure;
        }
        #endregion 召唤节点

    }
}