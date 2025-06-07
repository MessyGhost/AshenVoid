using System;

namespace AshenVoid.Core.Animation
{
    public delegate float AnimationCurve(float start, float end, float progress);

    public static class AnimationCurves
    {
        // 基础线性插值
        public static AnimationCurve Linear = (s, e, p) => s + (e - s) * p;

        // Sine 缓动
        public static AnimationCurve EaseInSine = (s, e, p) => EaseIn(s, e, p, MathF.Sin);
        public static AnimationCurve EaseOutSine = (s, e, p) => EaseOut(s, e, p, MathF.Sin);
        public static AnimationCurve EaseInOutSine = (s, e, p) => EaseInOut(s, e, p, MathF.Sin);

        // Quad 缓动
        public static AnimationCurve EaseInQuad = (s, e, p) => EaseIn(s, e, p, x => x * x);
        public static AnimationCurve EaseOutQuad = (s, e, p) => EaseOut(s, e, p, x => x * x);
        public static AnimationCurve EaseInOutQuad = (s, e, p) => EaseInOut(s, e, p, x => x * x);

        // Cubic 缓动
        public static AnimationCurve EaseInCubic = (s, e, p) => EaseIn(s, e, p, x => x * x * x);
        public static AnimationCurve EaseOutCubic = (s, e, p) => EaseOut(s, e, p, x => x * x * x);
        public static AnimationCurve EaseInOutCubic = (s, e, p) => EaseInOut(s, e, p, x => x * x * x);

        // Expo 缓动
        public static AnimationCurve EaseInExpo = (s, e, p) => EaseIn(s, e, p, x => x == 0 ? 0 : MathF.Pow(2, 10 * (x - 1)));
        public static AnimationCurve EaseOutExpo = (s, e, p) => EaseOut(s, e, p, x => x == 1 ? 1 : 1 - MathF.Pow(2, -10 * x));
        public static AnimationCurve EaseInOutExpo = (s, e, p) => EaseInOut(s, e, p, x =>
        {
            if (x == 0) return 0;
            if (x == 1) return 1;
            return x < 0.5f ? MathF.Pow(2, 20 * x - 10) / 2 : (2 - MathF.Pow(2, -20 * x + 10)) / 2;
        });

        // Circ 缓动
        public static AnimationCurve EaseInCirc = (s, e, p) => EaseIn(s, e, p, x => 1 - MathF.Sqrt(1 - x * x));
        public static AnimationCurve EaseOutCirc = (s, e, p) => EaseOut(s, e, p, x => MathF.Sqrt(1 - (x - 1) * (x - 1)));
        public static AnimationCurve EaseInOutCirc = (s, e, p) => EaseInOut(s, e, p, x =>
            x < 0.5f ? (1 - MathF.Sqrt(1 - (2 * x) * (2 * x))) / 2 : (1 + MathF.Sqrt(1 - (2 - 2 * x) * (2 - 2 * x))) / 2);

        // 内部辅助函数
        private static float EaseIn(float s, float e, float p, Func<float, float> func) =>
            s + (e - s) * func(p);
        private static float EaseOut(float s, float e, float p, Func<float, float> func) =>
            s + (e - s) * (1 - func(1 - p));
        private static float EaseInOut(float s, float e, float p, Func<float, float> func)
        {
            if (p < 0.5f)
                return s + (e - s) * (func(2 * p) / 2);
            else
                return s + (e - s) * (1 - func(2 * (1 - p)) / 2);
        }

        // 对称化扩展方法（支持正向/反向动画）
        public static AnimationCurve Symmetrize(this AnimationCurve curve)
        {
            return (s, e, p) =>
            {
                if (p < 0.5f)
                    return curve(s, e, p * 2f);
                else
                    return curve(e, s, (p - 0.5f) * 2f);
            };
        }
    }
}