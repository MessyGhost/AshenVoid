using AshenVoid.Core.DI;
using AshenVoid.Core.ECS.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// 唯一的组件管理者和依赖提供者。
    /// 负责动态发现、初始化和更新所有已注册的组件。
    /// </summary>
    public class ComponentController
    {
        private readonly List<IComponent> _components = new List<IComponent>();
        private readonly ServiceContainer _serviceContainer;

        public ComponentController(ServiceContainer serviceContainer)
        {
            _serviceContainer = serviceContainer;
        }

        /// <summary>
        /// 动态发现并初始化所有已注册的IComponent。
        /// </summary>
        public void Initialize()
        {
            // 动态地从DI容器中发现所有注册的IComponent服务
            var componentServices = _serviceContainer.GetAllServiceDescriptors()
                .Where(sd => typeof(IComponent).IsAssignableFrom(sd.ServiceType));

            foreach (var descriptor in componentServices)
            {
                var component = (IComponent)_serviceContainer.GetService(descriptor.ServiceType);
                _components.Add(component);
            }

            // 在解析完所有其他组件后，手动处理对自身的依赖
            _serviceContainer.RegisterInstance(this);

            // 初始化所有组件
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
        public T GetComponent<T>() where T : IComponent
        {
            // 查找实现了接口T的组件。
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