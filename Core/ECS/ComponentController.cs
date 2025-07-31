using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// Manages the lifecycle of all components attached to an NPC.
    /// </summary>
    public class ComponentController
    {
        private readonly Dictionary<Type, IComponent> _components = new Dictionary<Type, IComponent>();

        public ComponentController(NPC npc) { }

        public T AddComponent<T>(T component) where T : IComponent
        {
            _components[typeof(T)] = component;
            return component;
        }

        public T GetComponent<T>() where T : class, IComponent
        {
            return _components.TryGetValue(typeof(T), out var component) ? component as T : null;
        }

        /// <summary>
        /// Updates all registered components. This should be called from ModNPC.AI().
        /// </summary>
        public void Update()
        {
            foreach (var component in _components.Values)
            {
                component.Update();
            }
        }
    }
}