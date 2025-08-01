using System;
using System.Collections.Generic;

namespace AshenVoid.Core.ECS
{
    // 接口，用于非泛型地操作组件数组
    internal interface IComponentChunk
    {
        void AddComponent(IComponent component);
        void RemoveComponent(int index);
        void MoveComponent(int fromIndex, IComponentChunk toChunk);
        IComponent GetComponent(int index);
        Array GetComponentsAsArray();
    }

    // 泛型实现，存储实际的组件列表
    internal class ComponentChunk<T> : IComponentChunk where T : IComponent
    {
        public readonly List<T> Components = new();

        public void AddComponent(IComponent component) => Components.Add((T)component);
        public IComponent GetComponent(int index) => Components[index];
        public Array GetComponentsAsArray() => Components.ToArray();

        public void RemoveComponent(int index)
        {
            // 用最后一个元素填补空缺，以保持紧凑
            Components[index] = Components[^1];
            Components.RemoveAt(Components.Count - 1);
        }

        public void MoveComponent(int fromIndex, IComponentChunk toChunk)
        {
            ((ComponentChunk<T>)toChunk).Components.Add(Components[fromIndex]);
        }
    }

    public class Archetype
    {
        public readonly string Signature;
        public readonly HashSet<Type> ComponentTypes;
        private readonly Dictionary<Type, IComponentChunk> _componentChunks = new();

        // 实体ID -> 在组件数组中的索引
        private readonly Dictionary<int, int> _entityIdToIndex = new();
        // 索引 -> 实体ID
        private readonly List<int> _indexToEntityId = new();

        public IReadOnlyList<int> Entities => _indexToEntityId;

        public Archetype(HashSet<Type> componentTypes, string signature)
        {
            ComponentTypes = componentTypes;
            Signature = signature;
            foreach (var type in componentTypes)
            {
                var chunkType = typeof(ComponentChunk<>).MakeGenericType(type);
                _componentChunks[type] = (IComponentChunk)Activator.CreateInstance(chunkType);
            }
        }

        public void AddEntity(int entityId, IComponent[] components)
        {
            var index = _indexToEntityId.Count;
            _entityIdToIndex[entityId] = index;
            _indexToEntityId.Add(entityId);

            foreach (var component in components)
            {
                _componentChunks[component.GetType()].AddComponent(component);
            }
        }

        public void RemoveEntity(int entityId)
        {
            if (!_entityIdToIndex.TryGetValue(entityId, out var indexToRemove))
                return;

            // 将最后一个元素移动到当前位置来填补空缺
            int lastEntityId = _indexToEntityId[^1];
            _indexToEntityId[indexToRemove] = lastEntityId;
            _entityIdToIndex[lastEntityId] = indexToRemove;

            // 移除最后一个索引
            _indexToEntityId.RemoveAt(_indexToEntityId.Count - 1);
            _entityIdToIndex.Remove(entityId);

            // 在每个 chunk 中执行相同的交换移除操作
            foreach (var chunk in _componentChunks.Values)
            {
                chunk.RemoveComponent(indexToRemove);
            }
        }

        public T GetComponent<T>(int entityId) where T : class, IComponent
        {
            if (_entityIdToIndex.TryGetValue(entityId, out var index) && _componentChunks.TryGetValue(typeof(T), out var chunk))
            {
                return ((ComponentChunk<T>)chunk).GetComponent(index) as T;
            }
            return null;
        }

        public IComponent[] GetComponents(int entityId)
        {
            if (!_entityIdToIndex.TryGetValue(entityId, out var index))
                return Array.Empty<IComponent>();

            var components = new IComponent[ComponentTypes.Count];
            int i = 0;
            foreach (var chunk in _componentChunks.Values)
            {
                components[i++] = chunk.GetComponent(index);
            }
            return components;
        }

        public void MoveEntityTo(int entityId, Archetype newArchetype, IComponent newComponent = null)
        {
            if (!_entityIdToIndex.TryGetValue(entityId, out var fromIndex))
                return;

            // 1. 收集当前所有组件
            var componentsToMove = new List<IComponent>();
            foreach (var chunk in _componentChunks.Values)
            {
                componentsToMove.Add(chunk.GetComponent(fromIndex));
            }
            if (newComponent != null)
            {
                componentsToMove.Add(newComponent);
            }

            // 2. 添加到新 Archetype
            newArchetype.AddEntity(entityId, componentsToMove.ToArray());

            // 3. 从旧 Archetype 中移除
            RemoveEntity(entityId);
        }

        public bool Matches(HashSet<Type> requiredComponents)
        {
            return requiredComponents.IsSubsetOf(ComponentTypes);
        }
    }
}