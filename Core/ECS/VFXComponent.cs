using System.Collections.Generic;

namespace AshenVoid.Core.ECS
{
    public class VFXComponent : IComponent
    {
        public Queue<IVFXEffect> EffectQueue { get; } = new Queue<IVFXEffect>();
    }
}