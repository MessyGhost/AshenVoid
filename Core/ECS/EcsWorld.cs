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

        private readonly Dictionary<ArchetypeSignature, Archetype> _archetypes = new();
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
            var emptyArchetype = FindOrCreateArchetype(new HashSet<Type>());
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
            var signature = new ArchetypeSignature(componentTypes);
            if (_archetypes.TryGetValue(signature, out var archetype))
            {
                return archetype;
            }

            var newArchetype = new Archetype(componentTypes, signature);
            _archetypes[signature] = newArchetype;
            return newArchetype;
        }

        public T GetComponent<T>(int entityId) where T : class, IComponent
        {
            if (_entityArchetypes.TryGetValue(entityId, out var archetype))
            {
                return archetype.GetComponent<T>(entityId);
            }
            return null;
        }

        public bool TryGetComponent<T>(int entityId, out T component) where T : struct, IComponent
        {
            if (_entityArchetypes.TryGetValue(entityId, out var archetype))
            {
                return archetype.TryGetComponent(entityId, out component);
            }
            component = default;
            return false;
        }

        public void SetComponent<T>(int entityId, T component) where T : IComponent
        {
            if (_entityArchetypes.TryGetValue(entityId, out var archetype))
            {
                archetype.SetComponent(entityId, component);
            }
        }

        // This method is now less efficient and should be phased out.
        public List<int> GetEntities(HashSet<Type> requiredSet)
        {
            _entityListBuffer.Clear();
            if (!requiredSet.Any())
                return _entityListBuffer;

            var requiredSignature = new ArchetypeSignature(requiredSet);

            foreach (var archetype in _archetypes.Values)
            {
                if (archetype.Matches(requiredSignature))
                {
                    _entityListBuffer.AddRange(archetype.Entities);
                }
            }
            return _entityListBuffer;
        }

        public IEnumerable<Archetype> GetArchetypes(HashSet<Type> requiredSet)
        {
            if (!requiredSet.Any())
                yield break;

            var requiredSignature = new ArchetypeSignature(requiredSet);
            foreach (var archetype in _archetypes.Values)
            {
                if (archetype.Matches(requiredSignature))
                {
                    yield return archetype;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            _systemManager.Update(gameTime, this, _eventBus);
            IsDirty = false;
        }
    }
}