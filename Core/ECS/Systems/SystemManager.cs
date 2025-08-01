using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace AshenVoid.Core.ECS.Systems
{
    public class SystemManager
    {
        private readonly List<ISystem> _systems = new List<ISystem>();
        // Cache for performance
        private readonly Dictionary<ISystem, MethodInfo> _updateMethodCache = new Dictionary<ISystem, MethodInfo>();
        private readonly Dictionary<ISystem, ParameterInfo[]> _parameterCache = new Dictionary<ISystem, ParameterInfo[]>();

        public void RegisterSystem(ISystem system)
        {
            var updateMethod = system.GetType().GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            if (updateMethod == null)
            {
                ModContent.GetInstance<AshenVoid>().Logger.Warn($"System '{system.GetType().Name}' does not have a public 'Update' method and will not be executed.");
                return;
            }

            _systems.Add(system);
            _updateMethodCache[system] = updateMethod;
            _parameterCache[system] = updateMethod.GetParameters();
        }

        public void Update(GameTime gameTime, NPC npc, IReadOnlyDictionary<Type, IComponent> components)
        {
            foreach (var system in _systems)
            {
                var parameters = _parameterCache[system];
                var arguments = new object[parameters.Length];
                bool canExecute = true;

                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    if (param.ParameterType == typeof(GameTime))
                    {
                        arguments[i] = gameTime;
                    }
                    else if (param.ParameterType == typeof(NPC))
                    {
                        arguments[i] = npc;
                    }
                    else if (typeof(IComponent).IsAssignableFrom(param.ParameterType))
                    {
                        if (components.TryGetValue(param.ParameterType, out var component))
                        {
                            arguments[i] = component;
                        }
                        else
                        {
                            // If a required component is missing, skip this system update.
                            canExecute = false;
                            break;
                        }
                    }
                    else
                    {
                        // If we can't resolve a parameter, we can't execute the system.
                        ModContent.GetInstance<AshenVoid>().Logger.Warn($"System '{system.GetType().Name}' has an unresolvable parameter '{param.Name}' of type '{param.ParameterType.Name}'.");
                        canExecute = false;
                        break;
                    }
                }

                if (canExecute)
                {
                    _updateMethodCache[system].Invoke(system, arguments);
                }
            }
        }
    }
}