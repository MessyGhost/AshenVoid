using System;
using System.Collections.Generic;

namespace AshenVoid.Core
{
    /// <summary>
    /// A simple service locator for managing dependencies.
    /// This allows for a centralized way to register and resolve services like factories and loaders.
    /// </summary>
    public class ServiceLocator
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Registers a service instance.
        /// </summary>
        public void Register<T>(T service)
        {
            _services[typeof(T)] = service;
        }

        /// <summary>
        /// Resolves a service instance.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the service is not registered.</exception>
        public T Get<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            throw new InvalidOperationException($"Service of type {typeof(T).Name} not registered.");
        }
    }
}