using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace AshenVoid.Core.Tweening
{
    public class TweenManager : ModSystem
    {
        private static readonly List<Tween> _activeTweens = new();

        public static void Add(Tween tween)
        {
            _activeTweens.Add(tween);
        }

        public override void PostUpdateEverything()
        {
            if (_activeTweens.Count == 0) return;

            float deltaTime = (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;

            for (int i = _activeTweens.Count - 1; i >= 0; i--)
            {
                var tween = _activeTweens[i];
                tween.Update(deltaTime);
                if (tween.IsFinished)
                {
                    _activeTweens.RemoveAt(i);
                }
            }
        }

        public override void Unload()
        {
            _activeTweens.Clear();
        }
    }
}