using System;

namespace AshenVoid.Core.Tweening
{
    public class Tween
    {
        public float Duration { get; }
        public float Elapsed { get; private set; }
        public bool IsFinished => Elapsed >= Duration;

        private readonly Action<float> _onUpdate;
        private readonly EasingFunctions.Ease _ease;

        public Tween(float duration, Action<float> onUpdate, EasingFunctions.Ease ease = EasingFunctions.Ease.Linear)
        {
            Duration = duration;
            _onUpdate = onUpdate;
            _ease = ease;
        }

        public void Update(float deltaTime)
        {
            if (IsFinished) return;

            Elapsed += deltaTime;
            float t = Math.Clamp(Elapsed / Duration, 0f, 1f);
            float easedT = EasingFunctions.Get(_ease, t);
            _onUpdate(easedT);
        }
    }
}