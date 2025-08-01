using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace AshenVoid.Core.ECS.Systems
{
    public class VFXSystem : CachedComponentSystem
    {
        public override IEnumerable<Type> RequiredComponents => new[] { typeof(VFXComponent) };
        public override SystemExecutionSide ExecutionSide => SystemExecutionSide.Client;

        public override void UpdateEntity(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var vfxComponent = world.GetComponent<VFXComponent>(entityId);
            if (vfxComponent == null) return;

            for (int i = vfxComponent.ActiveEffects.Count - 1; i >= 0; i--)
            {
                var effect = vfxComponent.ActiveEffects[i];
                effect.Update();
                if (effect.IsFinished)
                {
                    vfxComponent.ActiveEffects.RemoveAt(i);
                }
            }
        }
    }
}