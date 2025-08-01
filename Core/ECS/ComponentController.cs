using AshenVoid.Core.ECS.Systems;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class ComponentController
    {
        private readonly Dictionary<Type, IComponent> _components = new Dictionary<Type, IComponent>();
        private readonly SystemManager _systemManager = new SystemManager();

        public void RegisterSystem(ISystem system)
        {
            _systemManager.RegisterSystem(system);
        }

        public void RegisterComponent(IComponent component)
        {
            _components[component.GetType()] = component;
        }

        public T GetComponent<T>() where T : class, IComponent
        {
            if (_components.TryGetValue(typeof(T), out var component))
            {
                return component as T;
            }
            return null;
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

        public void Update(GameTime gameTime, NPC npc, EventBus eventBus)
        {
            _systemManager.Update(gameTime, npc, this, eventBus);
        }
    }
}