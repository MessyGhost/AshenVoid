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

        private static readonly List<IComponent> s_emptyComponentList = new();
        private readonly List<int> _entityListBuffer = new(); // Reusable buffer for GetEntities

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
            emptyArchetype.AddEntity(entityId, s_emptyComponentList);
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

        // Obsolete but kept for compatibility. Creates a new HashSet.
        public IEnumerable<int> GetEntities(IEnumerable<Type> requiredComponents)
        {
            return GetEntities(new HashSet<Type>(requiredComponents));
        }

        // Optimized version that reuses the entity list buffer.
        public List<int> GetEntities(HashSet<Type> requiredSet)
        {
            _entityListBuffer.Clear();
            if (!requiredSet.Any())
                return _entityListBuffer;

            foreach (var archetype in _archetypes.Values)
            {
                if (archetype.Matches(requiredSet))
                {
                    _entityListBuffer.AddRange(archetype.Entities);
                }
            }
            return _entityListBuffer;
        }

        public void Update(GameTime gameTime)
        {
            _systemManager.Update(gameTime, this, _eventBus);
            IsDirty = false;
        }
    }
}