using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Systems;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace AshenVoid.Core.Builders
{
    public class BossBuilder
    {
        private readonly List<Func<IComponent>> _componentFactories = new List<Func<IComponent>>();
        private readonly List<ISystem> _systems = new List<ISystem>();
        private Type _initialStateType;

        // Constructor is still needed to pass context to the component factories if they need it.
        public BossBuilder() { }

        public BossBuilder WithInitialState(Type stateType)
        {
            if (!typeof(IState).IsAssignableFrom(stateType))
            {
                throw new ArgumentException($"{stateType.Name} does not implement IState.", nameof(stateType));
            }
            _initialStateType = stateType;
            return this;
        }

        public BossBuilder AddComponent(Func<IComponent> factory)
        {
            _componentFactories.Add(factory);
            return this;
        }

        public BossBuilder AddSystem(ISystem system)
        {
            _systems.Add(system);
            return this;
        }

        public ComponentController Build()
        {
            var controller = new ComponentController();

            foreach (var factory in _componentFactories)
            {
                controller.RegisterComponent(factory());
            }

            foreach (var system in _systems)
            {
                controller.RegisterSystem(system);
            }

            var aiState = controller.GetComponent<AIStateComponent>();
            if (aiState != null)
            {
                if (_initialStateType != null)
                {
                    aiState.SetInitialState(_initialStateType);
                }
                else
                {
                    ModContent.GetInstance<AshenVoid>().Logger.Warn("No initial state provided for the boss.");
                }
            }

            return controller;
        }
    }
}