using AshenVoid.Core.ECS.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace AshenVoid.Core.ECS
{
    public class VFXComponent : IVFXComponent
    {
        private readonly List<IVFXEffect> _activeEffects = new List<IVFXEffect>();

        public VFXComponent() { }

        public void AddEffect(IVFXEffect effect)
        {
            _activeEffects.Add(effect);
        }

        public void RemoveEffect<T>() where T : IVFXEffect
        {
            _activeEffects.RemoveAll(e => e is T);
        }

        public void Update()
        {
            foreach (var effect in _activeEffects)
            {
                effect.Update();
            }
            // Remove finished effects
            _activeEffects.RemoveAll(e => e.IsFinished);
        }

        public void PreDraw(SpriteBatch spriteBatch)
        {
            foreach (var effect in _activeEffects)
            {
                effect.PreDraw(spriteBatch);
            }
        }

        public void PostDraw(SpriteBatch spriteBatch)
        {
            foreach (var effect in _activeEffects)
            {
                effect.PostDraw(spriteBatch);
            }
        }
    }
}