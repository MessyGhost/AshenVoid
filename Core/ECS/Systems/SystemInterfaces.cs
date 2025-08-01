using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    /// <summary>
    /// Base marker interface for all systems.
    /// </summary>
    public interface ISystem { }

    /// <summary>
    /// System for handling NPC movement logic.
    /// </summary>
    public interface IMovementSystem : ISystem
    {
        void Update(GameTime gameTime, NPC npc, MovementComponent movementComponent, AIStateComponent aiState);
    }

    /// <summary>
    /// System for handling attack execution and cooldowns.
    /// </summary>
    public interface IAttackSystem : ISystem
    {
        void Update(GameTime gameTime, NPC npc, AttackComponent attackComponent, StatSheetComponent statSheet, AIStateComponent aiState);
    }

    /// <summary>
    /// System for updating NPC animation frames.
    /// </summary>
    public interface IAnimationSystem : ISystem
    {
        void Update(NPC npc, AnimationComponent animationComponent);
    }

    /// <summary>
    /// System for applying stat modifications to the NPC.
    /// </summary>
    public interface IStatSystem : ISystem
    {
        void Update(NPC npc, StatSheetComponent statSheet);
    }

    /// <summary>
    /// System for driving the AI state machine.
    /// </summary>
    public interface IAIStateSystem : ISystem
    {
        void Update(GameTime gameTime, NPC npc, AIStateComponent aiState);
    }

    /// <summary>
    /// System for monitoring health changes and publishing events.
    /// </summary>
    public interface IHealthSystem : ISystem
    {
        void Update(NPC npc, HealthComponent health, EventBus eventBus);
    }
}
