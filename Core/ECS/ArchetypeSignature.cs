using System;
using System.Collections.Generic;
using System.Linq;

namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// A struct representing the component composition of an archetype using a bitmask.
    /// This is more efficient for matching and storage than a string signature.
    /// </summary>
    public readonly struct ArchetypeSignature : IEquatable<ArchetypeSignature>
    {
        private const int BitsPerLong = 64;
        private readonly long[] _mask;

        public ArchetypeSignature(IEnumerable<Type> componentTypes)
        {
            int maxId = 0;
            var ids = new List<int>();
            foreach (var type in componentTypes)
            {
                var id = ComponentTypeManager.GetId(type);
                ids.Add(id);
                if (id > maxId) maxId = id;
            }

            _mask = new long[(maxId / BitsPerLong) + 1];
            foreach (var id in ids)
            {
                _mask[id / BitsPerLong] |= 1L << (id % BitsPerLong);
            }
        }

        public bool Matches(ArchetypeSignature requiredComponents)
        {
            if (requiredComponents._mask.Length > _mask.Length)
            {
                return false;
            }

            for (int i = 0; i < requiredComponents._mask.Length; i++)
            {
                if ((_mask[i] & requiredComponents._mask[i]) != requiredComponents._mask[i])
                {
                    return false;
                }
            }
            return true;
        }

        public bool Equals(ArchetypeSignature other)
        {
            if (_mask == null || other._mask == null) return _mask == other._mask;
            if (_mask.Length != other._mask.Length) return false;
            for (int i = 0; i < _mask.Length; i++)
            {
                if (_mask[i] != other._mask[i]) return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is ArchetypeSignature other && Equals(other);
        }

        public override int GetHashCode()
        {
            if (_mask == null) return 0;
            long hash = 0;
            for (int i = 0; i < _mask.Length; i++)
            {
                hash ^= _mask[i];
            }
            return hash.GetHashCode();
        }
    }
}