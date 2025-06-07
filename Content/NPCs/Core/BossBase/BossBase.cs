
using AshenVoid.Core.BehaviorTree;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace AshenVoid.Content.NPCs
{
    public abstract partial class BossBase : EnemyBase
    {
        // 阶段管理
        protected int currentPhase = 0;
        protected Dictionary<int, Node> phaseBehaviors = new Dictionary<int, Node>();

        // Boss状态
        protected bool isActive = false;
        protected bool hasSummonedMinions = false;

        // 移动系统相关
        protected SecondOrderDynamics _movementController;
        protected Vector2 snapshotPos;          // 用于保存玩家位置的快照
        protected Vector2 _currentTargetPos;      // 当前目标位置
        public Vector2 velocity;                // 计算得到的速度，用于倾斜动画

        // 动画相关
        protected float animationSpeedMultiplier = 1.0f; // 动画速度
        protected Vector2 _shakeOffset = Vector2.Zero;  // 用于抖动的位置偏移量

        // 行为树
        protected Node BehaviorTree;

        // 创建行为树（子类boss行为在这里编写）
        protected abstract void CreateBehaviorTree();

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

            // 使用二阶系统更新位置
            if (_movementController != null && TargetPlayer != null)
            {
                Vector2 smoothedPos = _movementController.Update(
                    (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds,
                    _currentTargetPos
                );

                // Main.NewText($"smoothedPos:{smoothedPos}");

                // 计算速度差值
                velocity = (smoothedPos - NPC.Center) / (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
                // Main.NewText((smoothedPos - NPC.Center) / (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds);
                NPC.Center = smoothedPos;

                // 应用 shake 偏移
                NPC.Center = smoothedPos + _shakeOffset;
            }

            BehaviorTree?.Evaluate();

            UpdateAnimationSpeed();

            HandleDespawn();
        }

        // boss出生时调用
        public virtual void ActivateBoss()
        {
            // 初始化控制器
            _movementController = new SecondOrderDynamics(
                frequency: 2f,      // 调整频率（越高越灵敏）
                dampingRatio: 0.7f, // 阻尼比（0.7-1.0防止振荡）
                responseScale: 1f   // 响应幅度
            );
            _movementController.Init(NPC.Center);
            _currentTargetPos = NPC.Center;
            isActive = true;
            NPC.TargetClosest(true);
            NPC.netUpdate = true;

            // 初始动画速度
            animationSpeedMultiplier = 3.0f;

            // 创造行为树
            CreateBehaviorTree();

            // 出生阶段
            ChangePhase(0);
        }

        // 更新动画速度
        private void UpdateAnimationSpeed()
        {
            if (animationSpeedMultiplier > 1.0f)
            {
                animationSpeedMultiplier -=
                    0.05f * (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds * 60f;
                if (animationSpeedMultiplier < 1.0f)
                    animationSpeedMultiplier = 1.0f;
            }
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

        // 倾斜动画相关逻辑
        public override void FindFrame(int frameHeight)
        {
            // 使用低通滤波平滑旋转角度
            float maxTiltAngle = MathHelper.ToRadians(30);
            float tiltFactor = 0.001f;
            float smoothingFactor = 0.1f; // 平滑因子，值越大过渡越平滑

            if (velocity.X != 0)
            {
                float tiltDirection = Math.Sign(velocity.X);
                float tiltMagnitude = Math.Min(Math.Abs(velocity.X) * tiltFactor, 1f);
                float targetRotation = tiltDirection * MathHelper.Lerp(0, maxTiltAngle, tiltMagnitude);

                // 使用线性插值平滑过渡
                NPC.rotation = MathHelper.Lerp(NPC.rotation, targetRotation, smoothingFactor);
            }
            else
            {
                // 平滑回归到0度
                NPC.rotation = MathHelper.Lerp(NPC.rotation, 0f, smoothingFactor);
            }

            NPC.spriteDirection = velocity.X > 0 ? 1 : -1;

            NPC.frameCounter += animationSpeedMultiplier;
            if (NPC.frameCounter >= 10f)
            {
                NPC.frameCounter = 0f;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) %
                             (Main.npcFrameCount[NPC.type] * frameHeight);
            }
        }
    }
}