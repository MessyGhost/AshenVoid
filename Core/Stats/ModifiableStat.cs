using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AshenVoid.Core.Stats
{
    public enum StatModType
    {
        Flat,       // Adds a flat value
        PercentAdd, // Adds a percentage of the base value
        PercentMult // Multiplies the final value by a percentage
    }

    public class StatModifier
    {
        public readonly float Value;
        public readonly StatModType Type;
        public readonly int Order;
        public readonly object Source;

        public StatModifier(float value, StatModType type, int order, object source)
        {
            Value = value;
            Type = type;
            Order = order;
            Source = source;
        }

        public StatModifier(float value, StatModType type) : this(value, type, (int)type, null) { }
    }

    public class ModifiableStat
    {
        public float BaseValue;
        private readonly List<StatModifier> _statModifiers;
        public readonly ReadOnlyCollection<StatModifier> StatModifiers;

        private bool _isDirty = true;
        private float _value;
        public float Value
        {
            get
            {
                if (_isDirty)
                {
                    _value = CalculateFinalValue();
                    _isDirty = false;
                }
                return _value;
            }
        }

        public ModifiableStat(float baseValue = 0)
        {
            BaseValue = baseValue;
            _statModifiers = new List<StatModifier>();
            StatModifiers = _statModifiers.AsReadOnly();
        }

        public void AddModifier(StatModifier mod)
        {
            _isDirty = true;
            _statModifiers.Add(mod);
            _statModifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        public bool RemoveModifier(StatModifier mod)
        {
            if (_statModifiers.Remove(mod))
            {
                _isDirty = true;
                return true;
            }
            return false;
        }

        public bool RemoveAllModifiersFromSource(object source)
        {
            int numRemovals = _statModifiers.RemoveAll(mod => mod.Source == source);
            if (numRemovals > 0)
            {
                _isDirty = true;
                return true;
            }
            return false;
        }

        private float CalculateFinalValue()
        {
            float finalValue = BaseValue;
            float sumPercentAdd = 0;

            foreach (var mod in _statModifiers)
            {
                if (mod.Type == StatModType.Flat)
                {
                    finalValue += mod.Value;
                }
                else if (mod.Type == StatModType.PercentAdd)
                {
                    sumPercentAdd += mod.Value;
                }
            }

            finalValue *= 1 + sumPercentAdd;

            foreach (var mod in _statModifiers)
            {
                if (mod.Type == StatModType.PercentMult)
                {
                    finalValue *= 1 + mod.Value;
                }
            }

            return (float)System.Math.Round(finalValue, 4);
        }
    }
}