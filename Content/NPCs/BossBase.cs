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

        // 移动系统相关
        protected SecondOrderDynamics _movementController;
        protected Vector2 snapshotPos;          // 用于保存玩家位置的快照
        protected Vector2 _currentTargetPos;      // 当前目标位置
        public Vector2 velocity;                // 计算得到的速度，用于倾斜动画

        // 动画相关
        protected float animationSpeedMultiplier = 1.0f;

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

            // 使用二阶系统更新位置
            if (_movementController != null && TargetPlayer != null)
            {
                Vector2 smoothedPos = _movementController.Update(
                    (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds,
                    _currentTargetPos
                );

                // 计算速度差值
                velocity = (smoothedPos - NPC.Center) / (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
                // Main.NewText((smoothedPos - NPC.Center) / (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds);
                NPC.Center = smoothedPos;
            }

            UpdateAnimationSpeed();

            base.AI();

            HandleDespawn();
        }

        // boss出生时调用
        public virtual void ActivateBoss()
        {
            // 初始化控制器
            _movementController = new SecondOrderDynamics(
                frequency: 2f,      // 调整频率（越高越灵敏）
                dampingRatio: 0.7f, // 阻尼比（0.7-1.0防止振荡）
                responseScale: 1f,  // 响应幅度
                initialPosition: NPC.Center
            );
            _currentTargetPos = NPC.Center;
            isActive = true;
            NPC.TargetClosest(true);
            NPC.netUpdate = true;

            // 初始动画速度
            animationSpeedMultiplier = 3.0f;
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

        // 移动函数，实质是设置目标位置和系统参数，
        protected Node MoveToPosition(Func<Vector2> target,
                                        float stopDistance = 10f,
                                        float f = 0.5f,
                                        float z = 1.0f,
                                        float r = 0)
        {
            return
            new SequenceNode(
                NodeBuilder.Once(
                    () =>
                    {
                        _currentTargetPos = target();
                        _movementController.SetConstants(f, z, r, NPC.Center);
                    }
                ),
                new ActionNode(() =>
                {
                    if (stopDistance > (_currentTargetPos - NPC.Center).Length())
                    {
                        Main.NewText($"Approach: {_currentTargetPos}");
                    }
                    return NodeState.Running;
                }
                )
            );
        }

        #region 移动预设方法
        /// <summary>
        /// 随机游荡（在指定范围内随机移动）
        /// </summary>
        private Vector2? wanderTarget;
        protected Node RandomWander(Vector2 center, float radius = 150f)
        {
            return new SequenceNode(
                NodeBuilder.Once(
                    () =>
                    {
                        // 生成新的随机目标点
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float distance = Main.rand.NextFloat(radius * 0.05f, radius);
                        wanderTarget = center + new Vector2(
                            (float)Math.Cos(angle) * distance,
                            (float)Math.Sin(angle) * distance
                        );
                    }
                ),
                MoveToPosition(() => wanderTarget.Value, 15f, 1f, 0.5f, 0)
            );
        }

        /// <summary>
        /// 快速冲刺移动（高加速度低阻尼）
        /// </summary>
        protected Node RushTowards(Vector2 target, float stopDistance = 20f)
        {
            return MoveToPosition(() => target, stopDistance,
                2f, 0.9f, 0.5f);
        }

        /// <summary>
        /// 缓慢接近（低速度高阻尼）
        /// </summary>
        protected Node CautiousApproach(Func<Vector2> target, float stopDistance = 30f)
        {
            return MoveToPosition(target, stopDistance, 10f,
                1f, 0.8f);
        }

        /// <summary>
        /// 环绕移动（围绕目标旋转）
        /// </summary>
        protected float orbitAngle = 0f;
        protected Node OrbitMovement(Func<Vector2> center, float radius = 200f, float angularSpeed = 0.05f)
        {
            return MoveToPosition(() =>
            {
                orbitAngle += angularSpeed;
                float x = center().X + (float)Math.Cos(orbitAngle) * radius;
                float y = center().Y + (float)Math.Sin(orbitAngle) * radius;
                return new Vector2(x, y);
            }, 15f, 18f, 1.5f, 0.6f);
        }

        /// <summary>
        /// 智能撤退（与玩家保持距离）
        /// </summary>
        protected Node IntelligentRetreat(Func<Vector2> target, float minDistance = 300f)
        {
            return MoveToPosition(() =>
            {
                Vector2 dir = NPC.Center - target();
                dir.Normalize();
                return target() + dir * minDistance;
            }, 20f, 12f, 1.2f, 0.7f);
        }

        #endregion

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


        // 动画相关逻辑
        public override void FindFrame(int frameHeight)
        {
            // 倾斜动画逻辑
            float maxTiltAngle = MathHelper.ToRadians(30);
            float tiltFactor = 0.001f;

            if (velocity.X != 0)
            {
                float tiltDirection = Math.Sign(velocity.X);
                float tiltMagnitude = Math.Min(Math.Abs(velocity.X) * tiltFactor, 1f);
                NPC.rotation = tiltDirection * MathHelper.Lerp(0, maxTiltAngle, tiltMagnitude);
            }
            else
            {
                NPC.rotation = 0;
            }

            NPC.spriteDirection = velocity.X > 0 ? 1 : -1;

            // 动画帧控制
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