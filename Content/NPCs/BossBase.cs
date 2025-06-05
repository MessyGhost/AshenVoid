using AshenVoid.Core.BehaviorTree;
using Microsoft.Xna.Framework;
using System;
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

        // 移动系统相关变量
        protected Vector2 snapshotPos;          // 用于保存玩家位置的快照
        private Vector2 _currentTargetPos;      // 当前目标位置
        private float _currentMaxSpeed = 15f;          // 当前最大速度
        private Vector2? _currentOmegaN;         // 当前X/Y轴自然频率
        private Vector2? _currentZeta;           // 当前X/Y轴阻尼比

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

        // 帧循环逻辑
        public override void AI()
        {
            // 首次激活检查
            if (!isActive && Main.netMode != NetmodeID.MultiplayerClient)
            {
                ActivateBoss();
            }

            UpdateMovement();
            base.AI();  // 执行行为树逻辑

            // Boss通用行为
            HandleDespawn();
        }

        // boss出生时调用，设置目标、网络和初始targetpos(设为boss当前位置，这个需要在覆写中先一步设置)
        public virtual void ActivateBoss()
        {
            _currentTargetPos = NPC.Center;
            isActive = true;
            NPC.TargetClosest(true);
            NPC.netUpdate = true;
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

        // 基于二阶PID系统的移动逻辑
        protected virtual void UpdateMovement()
        {
            // Main.NewText($"{_currentTargetPos}");
            // Main.NewText($"{NPC.Center}");
            Vector2 dir = _currentTargetPos - NPC.Center;
            float distance = dir.Length();
            if (distance == 0) return;
            dir.Normalize();

            // 分量计算加速度
            Vector2 desiredAcceleration = new Vector2(
                _currentOmegaN.Value.X * _currentOmegaN.Value.X * dir.X * distance - 2 * _currentZeta.Value.X * _currentOmegaN.Value.X * NPC.velocity.X,
                _currentOmegaN.Value.Y * _currentOmegaN.Value.Y * dir.Y * distance - 2 * _currentZeta.Value.Y * _currentOmegaN.Value.Y * NPC.velocity.Y
            );

            NPC.velocity += desiredAcceleration * (1f / 60f);
            NPC.position += NPC.velocity * (1f / 60f);

            if (NPC.velocity.Length() > _currentMaxSpeed)
            {
                NPC.velocity = Vector2.Normalize(NPC.velocity) * _currentMaxSpeed;
            }
        }
        protected NodeState MoveToPosition(Func<Vector2> target,
                                        float stopDistance = 10f,
                                        float maxSpeed = 15f,
                                        Vector2? omega_n = null,
                                        Vector2? zeta = null,
                                        bool faceTarget = true)
        {
            _currentTargetPos = target();
            _currentMaxSpeed = maxSpeed;
            _currentOmegaN = omega_n ?? new Vector2(2.0f);
            _currentZeta = zeta ?? new Vector2(0.7f);
            return stopDistance > (_currentTargetPos - NPC.Center).Length() ? NodeState.Success : NodeState.Running;
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