using AshenVoid.Core.BehaviorTree;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using static AshenVoid.Core.BehaviorTree.NodeBuilder;

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


        #region 移动预设
        /// <summary>
        /// 随机游荡（在指定范围内随机移动）
        /// </summary>
        private Vector2? wanderTarget;
        protected Node RandomWander(Func<Vector2> center, float radius = 150f)
        {
            return Sequence(
                Once(
                    () =>
                    {
                        // 生成新的随机目标点
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float distance = Main.rand.NextFloat(radius * 0.05f, radius);
                        wanderTarget = center() + new Vector2(
                            (float)Math.Cos(angle) * distance,
                            (float)Math.Sin(angle) * distance
                        );
                    }
                ),
                MoveToPosition(() => wanderTarget.Value, 15f, 0.5f, 0.3f, 0)
            );
        }

        /// <summary>
        /// 快速冲刺移动
        /// </summary>
        protected Node RushTowards(Func<Vector2> target, float stopDistance = 100f)
        {
            return MoveToPosition(() => target(), stopDistance,
                1.5f, 0.95f, -0.005f);
        }

        /// <summary>
        /// 缓慢接近
        /// </summary>
        protected Node CautiousApproach(Func<Vector2> target, float stopDistance = 100f)
        {
            return MoveToPosition(target, stopDistance,
                0.5f, 0.5f);
        }

        #endregion 移动预设

        #region 移动节点
        // 基于pid二阶系统的移动方法，传入targetpos产生阶跃，可修改系统相关函数
        protected Node MoveToPosition(Func<Vector2> target,
                                        float stopDistance = 10f,
                                        float f = 0.5f,
                                        float z = 1.0f,
                                        float r = 0)
        {
            return
            Sequence(
                Once(
                    () =>
                    {
                        _currentTargetPos = target();
                        _movementController.SetConstants(f, z, r, NPC.Center);
                        Main.NewText($"GetTarget:{_currentTargetPos}");
                    }
                ),
                Do(() =>
                {
                    if (stopDistance > (_currentTargetPos - NPC.Center).Length())
                    {
                        Main.NewText($"Approach: {_currentTargetPos}");
                        return NodeState.Success;
                    }
                    return NodeState.Running;
                }
                )
            );
        }
        #endregion 移动节点


        #region 动画预设

        public Node FadeIn(float duration = 1f, bool loop = false)
        {
            return new FadeInNode(this, duration, loop);
        }

        public Node FadeOut(float duration = 1f, bool loop = false)
        {
            return new FadeOutNode(this, duration, loop);
        }

        public Node Breathing(float duration = -1f, float speed = 1f, float scaleAmplitude = 0.05f)
        {
            return new BreathingAnimationNode(this, duration, speed, scaleAmplitude);
        }

        public Node Rotation(float targetAngle = MathHelper.PiOver2, float speed = 1f, bool loop = false)
        {
            return new RotationNode(this, targetAngle, speed, loop);
        }

        public Node Ghost(float duration = 2f, float interval = 0.2f)
        {
            return new GhostTrailNode(this, duration, interval);
        }

        public Node Flash(Color flashColor, float duration = 0.5f, int flashes = 1)
        {
            return new FlashNode(this, flashColor, duration, flashes);
        }

        public Node Blink(float frequency = 0.2f, float duration = 3f)
        {
            return new BlinkNode(this, frequency, duration);
        }

        public Node ColorShift(Color targetColor, float duration = 2f, ColorBlendMode blendMode = ColorBlendMode.Multiply)
        {
            return new ColorShiftNode(this, targetColor, duration, blendMode);
        }

        public Node Shake(float amplitude = 5f, float frequency = 10f, float duration = 2f)
        {
            return new ShakeNode(this, amplitude, frequency, duration);
        }

        #endregion 动画预设

        #region 动画节点
        // 淡入动画节点
        public class FadeInNode : Node
        {
            private readonly BossBase _boss;
            private readonly float _duration;
            private readonly bool _loop;
            private float _elapsedTime;

            public FadeInNode(BossBase boss, float duration = 1f, bool loop = false)
            {
                _boss = boss;
                _duration = duration;
                _loop = loop;
                _elapsedTime = 0f;
            }

            public override NodeState Evaluate()
            {
                var npc = _boss.NPC;
                float deltaTime = (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
                _elapsedTime += deltaTime;

                float alpha = MathHelper.Clamp(_elapsedTime / _duration, 0f, 1f);
                npc.alpha = (int)(255 * (1f - alpha));

                if (_elapsedTime >= _duration)
                {
                    if (_loop)
                        _elapsedTime = 0f;
                    else
                        return NodeState.Success;
                }

                return NodeState.Running;
            }

            public override void Reset()
            {
                _elapsedTime = 0f;
                base.Reset();
            }
        }

        // 淡出动画节点
        public class FadeOutNode : Node
        {
            private readonly BossBase _boss;
            private readonly float _duration;
            private readonly bool _loop;
            private float _elapsedTime;

            public FadeOutNode(BossBase boss, float duration = 1f, bool loop = false)
            {
                _boss = boss;
                _duration = duration;
                _loop = loop;
                _elapsedTime = 0f;
            }

            public override NodeState Evaluate()
            {
                var npc = _boss.NPC;
                float deltaTime = (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
                _elapsedTime += deltaTime;

                float alpha = MathHelper.Clamp(_elapsedTime / _duration, 0f, 1f);
                npc.alpha = (int)(255 * alpha);

                if (_elapsedTime >= _duration)
                {
                    if (_loop)
                        _elapsedTime = 0f;
                    else
                        return NodeState.Success;
                }

                return NodeState.Running;
            }

            public override void Reset()
            {
                _elapsedTime = 0f;
                base.Reset();
            }
        }

        // 呼吸动画节点
        public class BreathingAnimationNode : Node
        {
            private readonly BossBase _boss;
            private readonly float _speed;
            private readonly float _scaleAmplitude;
            private readonly float _duration; // 新增：持续时间（秒）

            private float _accumulatedTime;

            public BreathingAnimationNode(BossBase boss, float duration = -1f, float speed = 1f, float scaleAmplitude = 0.2f)
            {
                _boss = boss;
                _speed = speed;
                _scaleAmplitude = scaleAmplitude;
                _duration = duration;
            }

            public override NodeState Evaluate()
            {
                if (_boss.NPC == null)
                    return NodeState.Failure;

                var npc = _boss.NPC;
                float deltaTime = (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
                _accumulatedTime += deltaTime;

                // 如果设置了持续时间且已超时
                if (_duration > 0 && _accumulatedTime >= _duration)
                {
                    npc.scale = 1.0f; // 恢复原始缩放
                    _accumulatedTime = 0f;
                    return NodeState.Success;
                }

                // 计算当前缩放值
                float phase = MathHelper.TwoPi * (_accumulatedTime * _speed);
                float scale = 1.0f + _scaleAmplitude * (float)Math.Sin(phase);

                npc.scale = scale;
                return NodeState.Running;
            }

            public override void Reset()
            {
                base.Reset();
                _accumulatedTime = 0f;
            }
        }

        // 旋转动画节点
        public class RotationNode : Node
        {
            private readonly BossBase _boss;
            private readonly float _speed;
            private readonly bool _loop;
            private readonly float _targetAngle;
            private float _currentAngle;
            private float _elapsedTime;

            public RotationNode(BossBase boss, float targetAngle = MathHelper.PiOver2, float speed = 0.1f, bool loop = false)
            {
                _boss = boss;
                _targetAngle = targetAngle;
                _speed = speed;
                _loop = loop;
                _currentAngle = 0f;
                _elapsedTime = 0f;
            }

            public override NodeState Evaluate()
            {
                var npc = _boss.NPC;
                float deltaTime = (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
                _elapsedTime += deltaTime;

                float t = _loop ? _elapsedTime * _speed : MathHelper.Clamp(_elapsedTime * _speed, 0f, 1f);
                _currentAngle = MathHelper.Lerp(0f, _targetAngle, t);
                npc.rotation = _currentAngle;

                if (!_loop && t >= 1f)
                    return NodeState.Success;

                return NodeState.Running;
            }

            public override void Reset()
            {
                _currentAngle = 0f;
                _elapsedTime = 0f;
                base.Reset();
            }
        }

        // 虚影动画节点
        public class GhostTrailNode : Node
        {
            private readonly BossBase _boss;
            private readonly float _duration;
            private readonly float _interval;
            private float _elapsedTime;
            private float _lastGhostTime;

            public GhostTrailNode(BossBase boss, float duration = 2f, float interval = 0.2f)
            {
                _boss = boss;
                _duration = duration;
                _interval = interval;
                _elapsedTime = 0f;
                _lastGhostTime = 0f;
            }

            public override NodeState Evaluate()
            {
                var npc = _boss.NPC;
                float deltaTime = (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
                _elapsedTime += deltaTime;

                if (_elapsedTime >= _duration)
                    return NodeState.Success;

                _lastGhostTime += deltaTime;
                if (_lastGhostTime >= _interval)
                {
                    _lastGhostTime = 0f;
                    var ghost = new GhostTrail(npc.Center, npc.rotation, (byte)npc.alpha);
                    ghost.Draw();
                }

                return NodeState.Running;
            }

            public override void Reset()
            {
                _elapsedTime = 0f;
                _lastGhostTime = 0f;
                base.Reset();
            }
        }

        // 辅助类：GhostTrail
        public class GhostTrail
        {
            public Vector2 Position { get; set; }
            public float Rotation { get; set; }
            public byte Alpha { get; set; }

            public GhostTrail(Vector2 position, float rotation, byte alpha)
            {
                Position = position;
                Rotation = rotation;
                Alpha = alpha;
            }

            public void Draw()
            {
                Texture2D texture = TextureAssets.Npc[NPCID.EyeofCthulhu].Value;
                Color color = Color.White * (Alpha / 255f);
                Main.spriteBatch.Draw(texture, Position - Main.screenPosition, null, color, Rotation, texture.Size() / 2f, 1f, SpriteEffects.None, 0f);
            }
        }

        // 闪烁动画
        public class BlinkNode : Node
        {
            private readonly BossBase _boss;
            private readonly float _frequency;
            private readonly float _duration;
            private float _elapsedTime;
            private float _toggleTime;

            public BlinkNode(BossBase boss, float frequency = 0.2f, float duration = 3f)
            {
                _boss = boss;
                _frequency = frequency;
                _duration = duration;
                _elapsedTime = 0f;
                _toggleTime = 0f;
            }

            public override NodeState Evaluate()
            {
                var npc = _boss.NPC;
                float deltaTime = (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
                _elapsedTime += deltaTime;

                if (_elapsedTime >= _duration)
                    return NodeState.Success;

                _toggleTime += deltaTime;
                if (_toggleTime >= _frequency)
                {
                    _toggleTime = 0f;
                    npc.alpha = npc.alpha == 0 ? 255 : 0;
                }

                return NodeState.Running;
            }

            public override void Reset()
            {
                _elapsedTime = 0f;
                _toggleTime = 0f;
                base.Reset();
            }
        }


        // 用于颜色叠加效果的枚举
        public enum ColorBlendMode
        {
            Multiply,   // 正片叠底
            Additive,   // 叠加
            Overlay,    // 覆盖
            Screen,     // 屏幕
            Darken,     // 变暗
            Lighten,    // 变亮
            LinearDodge // 线性减淡
        }
        // 颜色渐变动画
        public class ColorShiftNode : Node
        {
            private readonly BossBase _boss;
            private readonly Color _targetColor;
            private readonly float _duration;
            private readonly ColorBlendMode _blendMode;

            private float _elapsedTime;
            private Color _startColor;

            public ColorShiftNode(BossBase boss, Color targetColor, float duration = 2f, ColorBlendMode blendMode = ColorBlendMode.Multiply)
            {
                _boss = boss;
                _targetColor = targetColor;
                _duration = duration;
                _blendMode = blendMode;
                _elapsedTime = 0f;
            }

            public override NodeState Evaluate()
            {
                var npc = _boss.NPC;
                if (npc == null)
                    return NodeState.Failure;

                if (_elapsedTime == 0f)
                {
                    _startColor = npc.color;
                }

                float deltaTime = (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
                _elapsedTime += deltaTime;

                float t = MathHelper.Clamp(_elapsedTime / _duration, 0f, 1f);
                Color currentColor = BlendColors(_startColor, _targetColor, t, _blendMode);

                npc.color = currentColor;

                if (_elapsedTime >= _duration)
                {
                    return NodeState.Success;
                }

                return NodeState.Running;
            }

            private Color BlendColors(Color baseColor, Color targetColor, float t, ColorBlendMode mode)
            {
                float r = baseColor.R / 255f;
                float g = baseColor.G / 255f;
                float b = baseColor.B / 255f;
                float a = baseColor.A / 255f;

                float tr = targetColor.R / 255f;
                float tg = targetColor.G / 255f;
                float tb = targetColor.B / 255f;
                float ta = targetColor.A / 255f;

                switch (mode)
                {
                    case ColorBlendMode.Multiply:
                        r = Lerp(r, r * tr, t);
                        g = Lerp(g, g * tg, t);
                        b = Lerp(b, b * tb, t);
                        a = Lerp(a, a * ta, t);
                        break;

                    case ColorBlendMode.Additive:
                        r = Lerp(r, Math.Min(1f, r + tr), t);
                        g = Lerp(g, Math.Min(1f, g + tg), t);
                        b = Lerp(b, Math.Min(1f, b + tb), t);
                        a = Lerp(a, Math.Min(1f, a + ta), t);
                        break;

                    case ColorBlendMode.Overlay:
                        r = Lerp(r, r < 0.5f ? 2 * r * tr : 1 - 2 * (1 - r) * (1 - tr), t);
                        g = Lerp(g, g < 0.5f ? 2 * g * tg : 1 - 2 * (1 - g) * (1 - tg), t);
                        b = Lerp(b, b < 0.5f ? 2 * b * tb : 1 - 2 * (1 - b) * (1 - tb), t);
                        a = Lerp(a, a, t);
                        break;

                    case ColorBlendMode.Screen:
                        r = Lerp(r, 1 - (1 - r) * (1 - tr), t);
                        g = Lerp(g, 1 - (1 - g) * (1 - tg), t);
                        b = Lerp(b, 1 - (1 - b) * (1 - tb), t);
                        a = Lerp(a, 1 - (1 - a) * (1 - ta), t);
                        break;

                    case ColorBlendMode.Darken:
                        r = Lerp(r, Math.Min(r, tr), t);
                        g = Lerp(g, Math.Min(g, tg), t);
                        b = Lerp(b, Math.Min(b, tb), t);
                        a = Lerp(a, Math.Min(a, ta), t);
                        break;

                    case ColorBlendMode.Lighten:
                        r = Lerp(r, Math.Max(r, tr), t);
                        g = Lerp(g, Math.Max(g, tg), t);
                        b = Lerp(b, Math.Max(b, tb), t);
                        a = Lerp(a, Math.Max(a, ta), t);
                        break;

                    case ColorBlendMode.LinearDodge:
                        r = Lerp(r, Math.Min(1f, r + tr), t);
                        g = Lerp(g, Math.Min(1f, g + tg), t);
                        b = Lerp(b, Math.Min(1f, b + tb), t);
                        a = Lerp(a, Math.Min(1f, a + ta), t);
                        break;

                    default:
                        r = Lerp(r, tr, t);
                        g = Lerp(g, tg, t);
                        b = Lerp(b, tb, t);
                        a = Lerp(a, ta, t);
                        break;
                }

                return new Color(
                    (byte)(r * 255),
                    (byte)(g * 255),
                    (byte)(b * 255),
                    (byte)(a * 255)
                );
            }

            private float Lerp(float a, float b, float t)
            {
                return a + (b - a) * t;
            }

            public override void Reset()
            {
                _elapsedTime = 0f;
                base.Reset();
            }
        }

        // 抖动动画
        public class ShakeNode : Node
        {
            private readonly BossBase _boss;
            private readonly float _amplitude;
            private readonly float _frequency;
            private readonly float _duration;
            private float _elapsedTime;
            private float _angle;

            public ShakeNode(BossBase boss, float amplitude = 5f, float frequency = 10f, float duration = 2f)
            {
                _boss = boss;
                _amplitude = amplitude;
                _frequency = frequency;
                _duration = duration;
                _elapsedTime = 0f;
                _angle = 0f;
            }

            public override NodeState Evaluate()
            {
                var npc = _boss.NPC;
                float deltaTime = (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
                _elapsedTime += deltaTime;

                if (_elapsedTime >= _duration)
                {
                    npc.Center = _boss.snapshotPos;
                    return NodeState.Success;
                }

                _angle += _frequency * deltaTime;
                float offsetX = (float)Math.Sin(_angle) * _amplitude;
                float offsetY = (float)Math.Cos(_angle) * _amplitude;
                npc.Center = _boss.snapshotPos + new Vector2(offsetX, offsetY);

                return NodeState.Running;
            }

            public override void Reset()
            {
                _elapsedTime = 0f;
                _angle = 0f;
                base.Reset();
            }
        }

        // 闪烁动画
        public class FlashNode : Node
        {
            private readonly BossBase _boss;
            private readonly Color _flashColor;
            private readonly float _duration;
            private readonly int _flashes;
            private float _elapsedTime;
            private Color _originalColor;

            public FlashNode(BossBase boss, Color flashColor, float duration = 0.5f, int flashes = 3)
            {
                _boss = boss;
                _flashColor = flashColor;
                _duration = duration;
                _flashes = flashes;
                _elapsedTime = 0f;
            }

            public override NodeState Evaluate()
            {
                var npc = _boss.NPC;
                if (npc == null)
                    return NodeState.Failure;

                if (_elapsedTime == 0f)
                {
                    _originalColor = npc.color;
                }

                float deltaTime = (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
                _elapsedTime += deltaTime;

                float totalFlashTime = _duration / _flashes;
                float flashCycle = _elapsedTime % totalFlashTime;
                float t = flashCycle / totalFlashTime;

                if (t < 0.5f)
                {
                    npc.color = Color.Lerp(_originalColor, _flashColor, t * 2f);
                }
                else
                {
                    npc.color = Color.Lerp(_flashColor, _originalColor, (t - 0.5f) * 2f);
                }

                if (_elapsedTime >= _duration)
                {
                    npc.color = _originalColor;
                    return NodeState.Success;
                }

                return NodeState.Running;
            }

            public override void Reset()
            {
                _elapsedTime = 0f;
                base.Reset();
            }
        }
        #endregion 动画节点


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