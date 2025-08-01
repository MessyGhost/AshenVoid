using AshenVoid.Core.ECS.Interfaces;
using AshenVoid.Core.ECS.Systems;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class ComponentController
    {
        private readonly Dictionary<Type, IComponent> _components = new Dictionary<Type, IComponent>();
        private readonly SystemManager _systemManager = new SystemManager();
        
        // NEW: Cache for networked components
        private List<INetworkedComponent> _networkedComponentsCache;

        public void RegisterSystem(ISystem system)
        {
            _systemManager.RegisterSystem(system);
        }

        public void RegisterComponent(IComponent component)
        {
            _components[component.GetType()] = component;
            
            // Invalidate the networked components cache
            _networkedComponentsCache = null;
        }

        public T GetComponent<T>() where T : class, IComponent
        {
            if (_components.TryGetValue(typeof(T), out var component))
            {
                return component as T;
            }
            return null;
        }

        public bool HasComponent(Type componentType)
        {
            return _components.ContainsKey(componentType);
        }

        public bool TryGetComponent<T>(out T result) where T : class, IComponent
        {
            if (_components.TryGetValue(typeof(T), out var component) && component is T casted)
            {
                result = casted;
                return true;
            }
            result = null;
            return false;
        }

        public IEnumerable<IComponent> GetAllComponents()
        {
            return _components.Values;
        }

        // NEW: Get only the components that need to be networked
        public IEnumerable<INetworkedComponent> GetNetworkedComponents()
        {
            if (_networkedComponentsCache == null)
            {
                _networkedComponentsCache = _components.Values.OfType<INetworkedComponent>().ToList();
            }
            return _networkedComponentsCache;
        }

        public void Update(GameTime gameTime, NPC npc, EventBus eventBus)
        {
            _systemManager.Update(gameTime, npc, this, eventBus);
        }

        public void BuildSystemCache()
        {
            _systemManager.BuildCache(this);
        }
    }
}