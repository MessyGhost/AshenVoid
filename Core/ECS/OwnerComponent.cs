namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// A component that marks an entity as being owned by another entity.
    /// Used for minions, projectiles, etc.
    /// </summary>
    public class OwnerComponent : IComponent
    {
        public int OwnerEntityId { get; set; }

        public OwnerComponent(int ownerEntityId)
        {
            OwnerEntityId = ownerEntityId;
        }
    }
}