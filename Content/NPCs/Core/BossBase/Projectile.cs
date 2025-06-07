using AshenVoid.Core.BehaviorTree;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace AshenVoid.Content.NPCs
{
    public abstract partial class BossBase
    {
        #region 弹幕节点
        // 发射弹幕
        protected NodeState ShootProjectile(int projectileType, Vector2 direction, float speed, int damage)
        {
            if (TargetPlayer == null) return NodeState.Failure;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center,
                    direction * speed, projectileType, damage, 5f, Main.myPlayer);
                return NodeState.Success;
            }
            return NodeState.Failure;
        }
        #endregion 弹幕节点
    }
}