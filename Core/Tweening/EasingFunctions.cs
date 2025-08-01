using System;

namespace AshenVoid.Core.Tweening
{
    public static class EasingFunctions
    {
        public enum Ease
        {
            Linear,
            EaseInQuad,
            EaseOutQuad,
            EaseInOutQuad
        }

        public static float Get(Ease ease, float t)
        {
            return ease switch
            {
                Ease.Linear => t,
                Ease.EaseInQuad => t * t,
                Ease.EaseOutQuad => t * (2 - t),
                Ease.EaseInOutQuad => t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t,
                _ => t,
            };
        }
    }
}