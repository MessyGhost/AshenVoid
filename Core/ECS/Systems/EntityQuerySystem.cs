using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AshenVoid.Core.ECS.Systems
{
    public abstract class EntityQuerySystem : ISystem
    {
        private readonly List<int> _cachedEntities = new();
        private readonly HashSet<Type> _requiredComponentsSet;
        private bool _isCacheInvalid = true;

        public abstract IEnumerable<Type> RequiredComponents { get; }
        public abstract SystemExecutionSide ExecutionSide { get; }

        protected EntityQuerySystem()
        {
            _requiredComponentsSet = new HashSet<Type>(RequiredComponents);
        }

        public virtual void UpdateAll(GameTime gameTime, EcsWorld world, EventBus eventBus)
        {
            if (world.IsDirty)
            {
                _isCacheInvalid = true;
            }

            if (_isCacheInvalid)
            {
                // Use the optimized GetEntities version
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

        public abstract void UpdateEntity(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus);
    }
}