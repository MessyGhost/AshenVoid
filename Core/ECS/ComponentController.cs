using AshenVoid.Core.ECS.Interfaces;
using AshenVoid.Core.ECS.Systems;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace AshenVoid.Core.ECS
{
    public class ComponentController
    {
        private readonly Dictionary<Type, IComponent> _components = new Dictionary<Type, IComponent>();
        private readonly SystemManager _systemManager = new SystemManager();
        private List<INetworkedComponent> _networkedComponentsCache;

        public void AddComponent(IComponent component)
        {
            _components[component.GetType()] = component;
            // Invalidate network cache if a new component is added
            _networkedComponentsCache = null;
        }

        public void AddSystem(ISystem system)
        {
            _systemManager.RegisterSystem(system);
        }

        public T GetComponent<T>() where T : class, IComponent
        {
            return _components.TryGetValue(typeof(T), out var component) ? (T)component : null;
        }

        public bool HasComponent<T>() where T : class, IComponent
        {
            return _components.ContainsKey(typeof(T));
        }

        public bool HasComponent(Type componentType)
        {
            return _components.ContainsKey(componentType);
        }

        public void BuildSystemCache()
        {
            _systemManager.BuildCache(this);
        }

        public void Update(GameTime gameTime, NPC npc, EventBus eventBus)
        {
            _systemManager.Update(gameTime, npc, this, eventBus);
        }

        public IEnumerable<INetworkedComponent> GetNetworkedComponents()
        {
            if (_networkedComponentsCache == null)
            {
                _networkedComponentsCache = _components.Values.OfType<INetworkedComponent>().ToList();
            }
            return _networkedComponentsCache;
        }
    }
}