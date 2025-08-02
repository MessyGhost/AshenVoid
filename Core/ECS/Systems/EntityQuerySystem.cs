using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AshenVoid.Core.ECS.Systems
{
    public abstract class EntityQuerySystem : ISystem
    {
        private readonly List<int> _cachedEntities = new();
        private readonly HashSet<Type> _requiredComponentsSet;
        private bool _isCacheInvalid = true;
        private readonly bool _archetypeUpdateOverridden;

        public abstract IEnumerable<Type> RequiredComponents { get; }
        public abstract SystemExecutionSide ExecutionSide { get; }

        protected EntityQuerySystem()
        {
            _requiredComponentsSet = new HashSet<Type>(RequiredComponents);

            // Check if the derived class has overridden the new UpdateArchetype method.
            var methodInfo = GetType().GetMethod(nameof(UpdateArchetype), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _archetypeUpdateOverridden = methodInfo.DeclaringType != typeof(EntityQuerySystem);
        }

        public virtual void UpdateAll(GameTime gameTime, EcsWorld world, EventBus eventBus)
        {
            if (world.IsDirty)
            {
                _isCacheInvalid = true;
            }

            if (_archetypeUpdateOverridden)
            {
                // New, high-performance path: iterate over archetypes
                var archetypes = world.GetArchetypes(_requiredComponentsSet);
                foreach (var archetype in archetypes)
                {
                    UpdateArchetype(gameTime, archetype, world, eventBus);
                }
            }
            else
            {
                // Old, compatible path: iterate over entities
                if (_isCacheInvalid)
                {
                    var entities = world.GetEntities(_requiredComponentsSet);
                    _cachedEntities.Clear();
                    _cachedEntities.AddRange(entities);
                    _isCacheInvalid = false;
                }

                foreach (var entityId in _cachedEntities)
                {
                    UpdateEntity(gameTime, entityId, world, eventBus);
                }
            }
        }

        /// <summary>
        /// Updates a single entity. This is the older, less-performant way of processing.
        /// It will be used if UpdateArchetype is not overridden.
        /// </summary>
        public virtual void UpdateEntity(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus) { }

        /// <summary>
        /// Updates all entities within a matching archetype at once.
        /// Override this for high-performance, cache-friendly, data-oriented processing.
        /// You can get component data as a Span from the archetype.
        /// </summary>
        protected virtual void UpdateArchetype(GameTime gameTime, Archetype archetype, EcsWorld world, EventBus eventBus) { }
    }
}