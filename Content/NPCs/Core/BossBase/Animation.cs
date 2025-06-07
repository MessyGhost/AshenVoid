using System;
using Terraria;
using AshenVoid.Core.BehaviorTree;
using AshenVoid.Core.Animation;
using Microsoft.Xna.Framework;
using static AshenVoid.Core.BehaviorTree.NodeBuilder;
using Steamworks;

namespace AshenVoid.Content.NPCs
{
    public abstract partial class BossBase
    {
        public static AnimationCurve DefaultCurve { get; set; } = AnimationCurves.EaseInOutCubic;

        #region 动画预设

        protected Node Alpha(int to, float duration = 1f, int? from = null, AnimationCurve curve = null)
        {
            return Animate(
                to / 255f,
                duration,
                from / 255f ?? NPC.alpha / 255f,
                curve ?? DefaultCurve,
                value => NPC.alpha = Math.Clamp((int)(value * 255), 0, 255),
                null
            );
        }

        protected Node FadeIn(float duration = 1.5f, AnimationCurve curve = null)
        {
            return Alpha(0, duration, 255, curve);
        }

        protected Node FadeOut(float duration = 1.5f, AnimationCurve curve = null)
        {
            return Alpha(255, duration, 0, curve);
        }

        protected Node Scale(float to, float duration = 1f, float? from = null, AnimationCurve curve = null)
        {
            return Animate(
                to,
                duration,
                from ?? NPC.scale,
                curve ?? DefaultCurve,
                value => NPC.scale = value,
                null
            );
        }

        protected Node Breathe(float duration = 1f, float minScale = 1f, float maxScale = 1.2f, AnimationCurve curve = null)
        {
            return Scale(maxScale, duration, minScale, (curve ?? DefaultCurve).Symmetrize());
        }

        protected Node Shake(float duration = 2f, float intensity = 2f)
        {
            return Animate(
                    duration,
                    progress =>
                    {
                        // 生成随机方向的扰动向量
                        Vector2 shake = Main.rand.NextVector2Circular(intensity, intensity);
                        _shakeOffset = shake; // 应用到 _shakeOffset
                    },
                    () =>
                    {
                        _shakeOffset = Vector2.Zero; // 动画结束后重置偏移
                    }
                );
        }

        protected Node Animate(float to, float duration, float from, AnimationCurve curve, Action<float> onUpdate, Action onComplete = null)
        {
            return Once(new CurveAnimationNode(duration, from, to, curve, onUpdate, onComplete));
        }

        protected Node Animate(float duration, Action<float> onUpdate, Action onComplete = null)
        {
            return Once(new CallbackAnimationNode(duration, onUpdate, onComplete));
        }

        #endregion

        #region 动画节点

        public class CurveAnimationNode : Node
        {
            private readonly float _duration;
            private readonly float _start;
            private readonly float _end;
            private readonly AnimationCurve _curve;
            private readonly Action<float> _onUpdate;
            private readonly Action _onComplete;

            private float _startTime;
            private bool _isStarted;

            public CurveAnimationNode(float duration, float start, float end, AnimationCurve curve, Action<float> onUpdate, Action onComplete = null)
            {
                _duration = duration;
                _start = start;
                _end = end;
                _curve = curve ?? DefaultCurve;
                _onUpdate = onUpdate;
                _onComplete = onComplete;
            }

            public override NodeState Evaluate()
            {
                if (!_isStarted)
                {
                    _startTime = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;
                    _isStarted = true;
                }

                float elapsed = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds - _startTime;
                float progress = Math.Clamp(elapsed / _duration, 0f, 1f);

                _onUpdate(_curve(_start, _end, progress));

                if (progress >= 1f)
                {
                    _onComplete?.Invoke();
                    _isStarted = false;
                    return NodeState.Success;
                }

                return NodeState.Running;
            }
        }

        public class CallbackAnimationNode : Node
        {
            private readonly float _duration;
            private readonly Action<float> _onUpdate;
            private readonly Action _onComplete;

            private float _startTime;
            private bool _isStarted;

            public CallbackAnimationNode(float duration, Action<float> onUpdate, Action onComplete = null)
            {
                _duration = duration;
                _onUpdate = onUpdate;
                _onComplete = onComplete;
            }

            public override NodeState Evaluate()
            {
                if (!_isStarted)
                {
                    _startTime = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;
                    _isStarted = true;
                }

                float elapsed = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds - _startTime;
                float progress = Math.Clamp(elapsed / _duration, 0f, 1f);

                _onUpdate(progress);

                if (progress >= 1f)
                {
                    _onComplete?.Invoke();
                    _isStarted = false;
                    return NodeState.Success;
                }

                return NodeState.Running;
            }
        }

        #endregion
    }
}