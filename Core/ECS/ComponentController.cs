using AshenVoid.Core.DI;
using AshenVoid.Core.ECS.Interfaces;
using AshenVoid.Core.ECS.Systems;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class ComponentController
    {
        private readonly List<IComponent> _components = new List<IComponent>();
        private readonly List<object> _allServices = new List<object>();
        private readonly ServiceContainer _serviceContainer;
        private readonly SystemManager _systemManager = new SystemManager();

        public ComponentController(ServiceContainer serviceContainer)
        {
            _serviceContainer = serviceContainer;
        }

        public void RegisterSystem(ISystem system)
        {
            _systemManager.RegisterSystem(system);
        }

        public void Initialize()
        {
            var allServiceDescriptors = _serviceContainer.GetAllServiceDescriptors();

            foreach (var descriptor in allServiceDescriptors)
            {
                var service = _serviceContainer.GetService(descriptor.ServiceType);
                _allServices.Add(service);
                if (service is IComponent component)
                {
                    _components.Add(component);
                }
            }

            _serviceContainer.RegisterInstance(this);

            foreach (var component in _components)
            {
                if (component is IInitializable initializable)
                {
                    initializable.Initialize();
                }
            }
        }

        public T GetComponent<T>() where T : class
        {
            return _allServices.OfType<T>().FirstOrDefault();
        }

        public void Update(GameTime gameTime, NPC npc)
        {
            foreach (var component in _components)
            {
                component.Update();
            }
            _systemManager.Update(gameTime, npc);
        }
    }
}