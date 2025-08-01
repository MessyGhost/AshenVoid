using System.Collections.Generic;

namespace AshenVoid.Core.ECS
{
    public class VFXComponent : IComponent
    {
        public readonly List<IVFXEffect> ActiveEffects = new();
    }
}