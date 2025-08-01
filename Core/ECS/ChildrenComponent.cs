using System.Collections.Generic;

namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// A component that holds a list of child entities owned by this entity.
    /// Used for bosses that summon minions.
    /// </summary>
    public class ChildrenComponent : IComponent
    {
        public readonly List<int> ChildEntityIds = new();
    }
}