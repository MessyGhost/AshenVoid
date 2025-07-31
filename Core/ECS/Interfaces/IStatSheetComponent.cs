using AshenVoid.Core.Stats;

namespace AshenVoid.Core.ECS.Interfaces
{
    public interface IStatSheetComponent : IComponent
    {
        ModifiableStat Damage { get; }
        ModifiableStat Defense { get; }
        ModifiableStat MovementSpeed { get; }
        ModifiableStat AttackCooldownMultiplier { get; }
        // Add other stats as needed
    }
}