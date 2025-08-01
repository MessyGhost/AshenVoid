using AshenVoid.Core.ECS.Systems;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AshenVoid.Core.ECS
{
    public class EcsWorld
    {
        private readonly SystemManager _systemManager;
        private readonly EventBus _eventBus;

        private readonly Dictionary<int, List<IComponent>> _entityComponents = new();
        private readonly Dictionary<int, Archetype> _entityArchetypes = new();
        private readonly List<Archetype> _archetypes = new();
        private int _nextEntityId = 0;

        public EcsWorld(SystemManager systemManager, EventBus eventBus)
        {
            _systemManager = systemManager;
            _eventBus = eventBus;
        }

        public int CreateEntity()
        {
            int entityId = _nextEntityId++;
            _entityComponents[entityId] = new List<IComponent>();
            UpdateArchetype(entityId);
            return entityId;
        }

        public void AddComponent(int entityId, IComponent component)
        {
            if (_entityComponents.ContainsKey(entityId))
            {
                _entityComponents[entityId].Add(component);
                UpdateArchetype(entityId);
            }
        }

        private void UpdateArchetype(int entityId)
        {
            var currentArchetype = _entityArchetypes.GetValueOrDefault(entityId);
            currentArchetype?.Entities.Remove(entityId);

            var componentTypes = new HashSet<Type>(_entityComponents[entityId].Select(c => c.GetType()));
            var newArchetype = FindOrCreateArchetype(componentTypes);

            newArchetype.Entities.Add(entityId);
            _entityArchetypes[entityId] = newArchetype;
        }

        private Archetype FindOrCreateArchetype(HashSet<Type> componentTypes)
        {
            foreach (var archetype in _archetypes)
            {
                if (archetype.ComponentTypes.SetEquals(componentTypes))
                {
                    return archetype;
                }
            }

            var newArchetype = new Archetype(componentTypes);
            _archetypes.Add(newArchetype);
            return newArchetype;
        }

        public T GetComponent<T>(int entityId) where T : class, IComponent
        {
            if (_entityComponents.TryGetValue(entityId, out var components))
            {
                return components.OfType<T>().FirstOrDefault();
            }
            return null;
        }

        public IEnumerable<int> GetEntities(IEnumerable<Type> requiredComponents)
        {
            var requiredSet = new HashSet<Type>(requiredComponents);
            if (!requiredSet.Any())
                yield break;

            foreach (var archetype in _archetypes)
            {
                if (archetype.Matches(requiredSet))
                {
                    foreach (var entityId in archetype.Entities)
                    {
                        yield return entityId;
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            _systemManager.Update(gameTime, this, _eventBus);
        }
    }
}