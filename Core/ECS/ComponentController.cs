using AshenVoid.Core.DI;
using AshenVoid.Core.ECS.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// 唯一的组件管理者和依赖提供者。
    /// 负责创建、初始化和提供对所有组件的访问。
    /// </summary>
    public class ComponentController
    {
        private readonly List<IComponent> _components = new List<IComponent>();
        private readonly ServiceContainer _serviceContainer;
        private readonly NPC _npc;

        public ComponentController(NPC npc, ServiceContainer serviceContainer)
        {
            _npc = npc;
            _serviceContainer = serviceContainer;
        }

        /// <summary>
        /// 创建所有已注册的组件实例。
        /// </summary>
        public void Initialize()
        {
            // The controller is now responsible for creating components.
            _components.Add(_serviceContainer.GetService<IMovementComponent>());
            _components.Add(_serviceContainer.GetService<IAttackComponent>());
            _components.Add(_serviceContainer.GetService<IAnimationComponent>());
            _components.Add(_serviceContainer.GetService<IVFXComponent>());
            _components.Add(_serviceContainer.GetService<IStatSheetComponent>());

            // AIComponent has a dependency on ComponentController, so we register this instance
            // into the container before resolving AIComponent to break the circular dependency.
            _serviceContainer.RegisterInstance(this);
            _components.Add(_serviceContainer.GetService<AIComponent>());

            // After all components are created, initialize them.
            foreach (var component in _components)
            {
                if (component is IInitializable initializable)
                {
                    initializable.Initialize();
                }
            }
        }

        /// <summary>
        /// 按接口类型 T 从列表中查找并返回组件实例。
        /// </summary>
        public T GetComponent<T>() where T : class, IComponent
        {
            // Search for a component that implements the interface T.
            return _components.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// 更新所有注册的组件。
        /// </summary>
        public void Update()
        {
            foreach (var component in _components)
            {
                component.Update();
            }
        }
    }
}