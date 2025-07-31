using AshenVoid.Core.DI;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace AshenVoid.Core.Builders
{
    /// <summary>
    /// 使用构建器模式来配置和组装一个拥有ECS架构的Boss。
    /// 这是一个流式API，用于解耦Boss初始化过程。
    /// </summary>
    public class BossBuilder
    {
        private readonly ModNPC _npc;
        private readonly ServiceContainer _serviceContainer = new();
        private readonly List<Action<ServiceContainer>> _registrationActions = new();
        private IState _initialState;

        public BossBuilder(ModNPC npc)
        {
            _npc = npc;
            // 注册NPC实例本身，它是一个基本依赖。
            _serviceContainer.RegisterInstance(npc);
        }

        /// <summary>
        /// 注册一个或多个服务/组件到DI容器。
        /// </summary>
        /// <param name="registrationAction">一个委托，用于在容器中注册服务。</param>
        public BossBuilder WithServices(Action<ServiceContainer> registrationAction)
        {
            _registrationActions.Add(registrationAction);
            return this;
        }

        /// <summary>
        /// 设置AI状态机的初始状态。
        /// </summary>
        /// <param name="state">初始状态。</param>
        public BossBuilder WithInitialState(IState state)
        {
            _initialState = state;
            return this;
        }

        /// <summary>
        /// 在构建配置期间，从内部DI容器解析一个已注册的服务。
        /// 主要用于在创建状态等对象时解决依赖。
        /// </summary>
        public T GetService<T>() where T : class
        {
            return _serviceContainer.GetService<T>();
        }

        /// <summary>
        /// 完成构建过程，返回完全初始化和配置好的组件控制器。
        /// </summary>
        public ComponentController Build()
        {
            // 执行所有注册操作来填充容器。
            foreach (var action in _registrationActions)
            {
                action(_serviceContainer);
            }

            // 让容器自己创建ComponentController。
            _serviceContainer.RegisterSingleton<ComponentController, ComponentController>();
            var controller = _serviceContainer.GetService<ComponentController>();

            // 初始化所有组件。
            controller.Initialize();

            // 为AI设置初始状态。
            var ai = controller.GetComponent<AIComponent>();
            if (ai != null && _initialState != null)
            {
                ai.SetInitialState(_initialState);
            }
            else if (_initialState != null)
            {
                ModContent.GetInstance<AshenVoid>().Logger.Warn("提供了初始状态，但未找到AIComponent来管理它。");
            }

            return controller;
        }
    }
}