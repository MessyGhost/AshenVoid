using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using AshenVoid.Core.Events;

namespace AshenVoid.Core.ECS.Systems
{
    public class SystemManager
    {
        private readonly List<IAttackSystem> _attackSystems = new List<IAttackSystem>();
        private readonly List<IMovementSystem> _movementSystems = new List<IMovementSystem>();
        private readonly List<IAnimationSystem> _animationSystems = new List<IAnimationSystem>();
        private readonly List<IStatSystem> _statSystems = new List<IStatSystem>();
        private readonly List<IAIStateSystem> _aiStateSystems = new List<IAIStateSystem>();
        private readonly List<IHealthSystem> _healthSystems = new List<IHealthSystem>();
        private readonly List<ISystem> _otherSystems = new List<ISystem>(); // For systems without a specific interface yet

        public void RegisterSystem(ISystem system)
        {
            if (system is IAttackSystem attackSystem)
                _attackSystems.Add(attackSystem);
            else if (system is IMovementSystem movementSystem)
                _movementSystems.Add(movementSystem);
            else if (system is IAnimationSystem animationSystem)
                _animationSystems.Add(animationSystem);
            else if (system is IStatSystem statSystem)
                _statSystems.Add(statSystem);
            else if (system is IAIStateSystem aiStateSystem)
                _aiStateSystems.Add(aiStateSystem);
            else if (system is IHealthSystem healthSystem)
                _healthSystems.Add(healthSystem);
            else
                _otherSystems.Add(system);
        }

        public void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus)
        {
            // It's more efficient to get components once, if they exist.
            controller.TryGetComponent(out MovementComponent movement);
            controller.TryGetComponent(out AttackComponent attack);
            controller.TryGetComponent(out AnimationComponent animation);
            controller.TryGetComponent(out StatSheetComponent statSheet);
            controller.TryGetComponent(out AIStateComponent aiState);
            controller.TryGetComponent(out HealthComponent health);

            // Execute systems only if they have all their required components.
            if (health != null)
            {
                foreach (var system in _healthSystems)
                    system.Update(npc, health, eventBus);
            }

            if (movement != null && aiState != null)
            {
                foreach (var system in _movementSystems)
                    system.Update(gameTime, npc, movement, aiState);
            }

            if (attack != null && statSheet != null && aiState != null)
            {
                foreach (var system in _attackSystems)
                    system.Update(gameTime, npc, attack, statSheet, aiState);
            }

            if (animation != null)
            {
                foreach (var system in _animationSystems)
                    system.Update(npc, animation);
            }

            if (statSheet != null)
            {
                foreach (var system in _statSystems)
                    system.Update(npc, statSheet);
            }

            if (aiState != null)
            {
                foreach (var system in _aiStateSystems)
                    system.Update(gameTime, npc, aiState);
            }
        }
    }
}