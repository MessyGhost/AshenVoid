using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.AI;
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
        private readonly Dictionary<string, object> _blackboardData = new Dictionary<string, object>();
        private Type _initialStateType;
        private Action<ComponentController> _onBuildCallback;

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

        public BossBuilder WithBlackboardData(string key, object value)
        {
            _blackboardData[key] = value;
            return this;
        }

        public BossBuilder OnBuild(Action<ComponentController> callback)
        {
            _onBuildCallback = callback;
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
                // Populate blackboard with initial data
                foreach (var data in _blackboardData)
                {
                    aiState.Blackboard.Set(data.Key, data.Value);
                }

                if (_initialStateType != null)
                {
                    aiState.SetInitialState(_initialStateType);
                }
                else
                {
                    ModContent.GetInstance<AshenVoid>().Logger.Warn("No initial state provided for the boss.");
                }
            }

            _onBuildCallback?.Invoke(controller);

            return controller;
        }
    }
}