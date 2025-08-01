using AshenVoid.Core.ECS.Systems;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AshenVoid.Core.ECS
{
    public class EcsWorld
    {
        private readonly SystemManager _systemManager;
        private readonly EventBus _eventBus;

        private readonly Dictionary<string, Archetype> _archetypes = new();
        private readonly Dictionary<int, Archetype> _entityArchetypes = new();
        private int _nextEntityId = 0;

        public bool IsDirty { get; private set; } = true;

        public EcsWorld(SystemManager systemManager, EventBus eventBus)
        {
            _systemManager = systemManager;
            _eventBus = eventBus;
            // Create the initial empty archetype
            FindOrCreateArchetype(new HashSet<Type>());
        }

        public int CreateEntity()
        {
            int entityId = _nextEntityId++;
            var emptyArchetype = _archetypes[""];
            emptyArchetype.AddEntity(entityId, Array.Empty<IComponent>());
            _entityArchetypes[entityId] = emptyArchetype;
            IsDirty = true;
            return entityId;
        }

        public void DestroyEntity(int entityId)
        {
            if (_entityArchetypes.TryGetValue(entityId, out var archetype))
            {
                archetype.RemoveEntity(entityId);
                _entityArchetypes.Remove(entityId);
                IsDirty = true;
            }
        }

        public void AddComponent(int entityId, IComponent component)
        {
            if (!_entityArchetypes.TryGetValue(entityId, out var oldArchetype))
                return;

            var newComponentTypes = new HashSet<Type>(oldArchetype.ComponentTypes);
            newComponentTypes.Add(component.GetType());

            var newArchetype = FindOrCreateArchetype(newComponentTypes);

            oldArchetype.MoveEntityTo(entityId, newArchetype, component);
            _entityArchetypes[entityId] = newArchetype;
            IsDirty = true;
        }

        private Archetype FindOrCreateArchetype(HashSet<Type> componentTypes)
        {
            var signature = GenerateArchetypeSignature(componentTypes);
            if (_archetypes.TryGetValue(signature, out var archetype))
            {
                return archetype;
            }

            var newArchetype = new Archetype(componentTypes, signature);
            _archetypes[signature] = newArchetype;
            return newArchetype;
        }

        private string GenerateArchetypeSignature(HashSet<Type> componentTypes)
        {
            if (componentTypes == null || componentTypes.Count == 0)
                return "";

            var typeNames = componentTypes.Select(t => t.FullName).ToList();
            typeNames.Sort(StringComparer.Ordinal);

            var sb = new StringBuilder();
            foreach (var name in typeNames)
            {
                sb.Append(name).Append(';');
            }
            return sb.ToString();
        }

        public T GetComponent<T>(int entityId) where T : class, IComponent
        {
            if (_entityArchetypes.TryGetValue(entityId, out var archetype))
            {
                return archetype.GetComponent<T>(entityId);
            }
            return null;
        }

        public IEnumerable<int> GetEntities(IEnumerable<Type> requiredComponents)
        {
            var requiredSet = new HashSet<Type>(requiredComponents);
            if (!requiredSet.Any())
                return Enumerable.Empty<int>();

            var matchingEntities = new List<int>();
            foreach (var archetype in _archetypes.Values)
            {
                if (archetype.Matches(requiredSet))
                {
                    matchingEntities.AddRange(archetype.Entities);
                }
            }
            return matchingEntities;
        }

        public void Update(GameTime gameTime)
        {
            _systemManager.Update(gameTime, this, _eventBus);
            IsDirty = false;
        }
    }
}