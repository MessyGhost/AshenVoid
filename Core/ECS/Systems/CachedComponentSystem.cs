using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AshenVoid.Core.ECS.Systems
{
    public abstract class CachedComponentSystem : IComponentSystem
    {
        private readonly List<int> _cachedEntities = new();
        private bool _isCacheInvalid = true;

        public abstract IEnumerable<Type> RequiredComponents { get; }
        public abstract SystemExecutionSide ExecutionSide { get; }

        public void Update(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            // This method is called by the SystemManager for each entity, which we want to avoid.
            // The logic is moved to a new UpdateAll method.
            // This is a design flaw in the current SystemManager.
            // For now, we will leave this empty and do the work in a different way.
        }

        public virtual void UpdateAll(GameTime gameTime, EcsWorld world, EventBus eventBus)
        {
            if (world.IsDirty)
            {
                _isCacheInvalid = true;
            }

            if (_isCacheInvalid)
            {
                _cachedEntities.Clear();
                _cachedEntities.AddRange(world.GetEntities(RequiredComponents));
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