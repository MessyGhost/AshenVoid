using AshenVoid.Core.DI;
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
        private readonly ModNPC _npc;
        private readonly ServiceContainer _serviceContainer = new();
        private readonly List<Action<ServiceContainer>> _registrationActions = new();
        private IState _initialState;

        public BossBuilder(ModNPC npc)
        {
            _npc = npc;
            _serviceContainer.RegisterInstance(npc);
        }

        public BossBuilder WithServices(Action<ServiceContainer> registrationAction)
        {
            _registrationActions.Add(registrationAction);
            return this;
        }

        public BossBuilder WithInitialState(IState state)
        {
            _initialState = state;
            return this;
        }

        public T GetService<T>() where T : class
        {
            return _serviceContainer.GetService<T>();
        }

        public ComponentController Build()
        {
            foreach (var action in _registrationActions)
            {
                action(_serviceContainer);
            }

            _serviceContainer.RegisterSingleton<ComponentController, ComponentController>();
            var controller = _serviceContainer.GetService<ComponentController>();

            // Register systems
            controller.RegisterSystem(new MovementSystem());
            controller.RegisterSystem(new AttackSystem());

            controller.Initialize();

            var aiState = controller.GetComponent<AIStateComponent>();
            if (aiState != null && _initialState != null)
            {
                aiState.SetInitialState(_initialState);
            }
            else if (_initialState != null)
            {
                ModContent.GetInstance<AshenVoid>().Logger.Warn("提供了初始状态，但未找到AIStateComponent来管理它。");
            }

            return controller;
        }
    }
}