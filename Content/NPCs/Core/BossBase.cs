using AshenVoid.Core.BehaviorTree;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace AshenVoid.Content.NPCs
{
    public abstract class BossBase : EnemyBase
    {
        // 阶段管理
        protected int currentPhase = 0;
        protected Dictionary<int, Node> phaseBehaviors = new Dictionary<int, Node>();

        // Boss状态
        protected bool isActive = false;
        protected bool hasSummonedMinions = false;

        // 初始化行为树（子类必须实现）
        protected abstract void CreateBehaviorTree();

        protected override void InitializeBehaviorTree()
        {
            CreateBehaviorTree();

            // 设置初始为第一阶段
            ChangePhase(0);
        }

        // 切换Boss阶段
        public virtual void ChangePhase(int newPhase)
        {
            if (phaseBehaviors.TryGetValue(newPhase, out Node newBehavior))
            {
                Main.NewText($"阶段切换: {currentPhase} -> {newPhase}");
                currentPhase = newPhase;
                BehaviorTree = newBehavior;
                OnPhaseChanged();
            }
        }

        // 阶段切换时的自定义逻辑
        protected virtual void OnPhaseChanged() { }

        // 激活Boss
        public virtual void ActivateBoss()
        {
            isActive = true;
            NPC.TargetClosest(true);
            NPC.netUpdate = true;
        }

        public override void AI()
        {
            // 首次激活检查
            if (!isActive && Main.netMode != NetmodeID.MultiplayerClient)
            {
                ActivateBoss();
            }

            base.AI();  // 执行行为树逻辑

            // Boss通用行为
            HandleDespawn();
        }

        // 防止Boss在玩家死亡后消失
        protected virtual void HandleDespawn()
        {
            if (TargetPlayer != null && TargetPlayer.dead)
            {
                NPC.TargetClosest(false);
                if (NPC.target < 0 || NPC.target >= Main.maxPlayers || Main.player[NPC.target].dead)
                {
                    NPC.velocity.Y += 0.1f;
                    if (NPC.timeLeft > 60)
                    {
                        NPC.timeLeft = 60;
                    }
                }
            }
        }

        // ===== 常用行为节点 =====

        // 追逐目标玩家
        protected NodeState ChaseTarget(float r)
        {
            // Main.NewText("ChaseTarget");
            if (TargetPlayer == null) return NodeState.Failure;

            Vector2 direction = TargetPlayer.Center - NPC.Center;

            if (direction.Length() < r) return NodeState.Success;// r为追及半径，到达后算作追及成功

            direction.Normalize();

            NPC.velocity = (NPC.velocity * 15f + direction * 8f) / 16f;
            NPC.velocity = Vector2.Clamp(NPC.velocity, -Vector2.One * 10f, Vector2.One * 10f);

            return NodeState.Running;
        }

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
    }
}