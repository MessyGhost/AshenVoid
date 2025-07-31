using System;
using System.Collections.Generic;
using System.Linq;

namespace AshenVoid.Core.DI
{
    public enum ServiceLifetime
    {
        Singleton,
        Transient
    }

    public class ServiceDescriptor
    {
        public Type ServiceType { get; set; }
        public Type ImplementationType { get; set; }
        public ServiceLifetime Lifetime { get; set; }
    }

    public class ServiceNotFoundException : Exception
    {
        public ServiceNotFoundException(string message) : base(message) { }
    }

    public class ServiceContainer
    {
        private readonly Dictionary<Type, ServiceDescriptor> _services = new();
        private readonly Dictionary<Type, object> _singletonInstances = new();

        public void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface
            where TInterface : class
        {
            Register<TInterface, TImplementation>(ServiceLifetime.Singleton);
        }

        public void RegisterTransient<TInterface, TImplementation>()
            where TImplementation : class, TInterface
            where TInterface : class
        {
            Register<TInterface, TImplementation>(ServiceLifetime.Transient);
        }

        private void Register<TInterface, TImplementation>(ServiceLifetime lifetime)
            where TImplementation : class, TInterface
            where TInterface : class
        {
            _services[typeof(TInterface)] = new ServiceDescriptor
            {
                ServiceType = typeof(TInterface),
                ImplementationType = typeof(TImplementation),
                Lifetime = lifetime
            };
        }

        public void RegisterInstance<T>(T instance) where T : class
        {
            var type = typeof(T);
            _services[type] = new ServiceDescriptor
            {
                ServiceType = type,
                ImplementationType = type,
                Lifetime = ServiceLifetime.Singleton
            };
            _singletonInstances[type] = instance;
        }

        public T GetService<T>() where T : class
        {
            return (T)GetService(typeof(T));
        }

        public object GetService(Type serviceType)
        {
            if (!_services.TryGetValue(serviceType, out var descriptor))
            {
                // A special case for registering the NPC instance itself
                if (_singletonInstances.TryGetValue(serviceType, out var instance))
                    return instance;

                throw new ServiceNotFoundException($"服务未注册: {serviceType.Name}");
            }

            if (descriptor.Lifetime == ServiceLifetime.Singleton)
            {
                if (_singletonInstances.TryGetValue(serviceType, out var instance))
                    return instance;

                instance = CreateInstance(descriptor.ImplementationType);
                _singletonInstances[serviceType] = instance;
                return instance;
            }

            return CreateInstance(descriptor.ImplementationType);
        }

        private object CreateInstance(Type type)
        {
            var constructors = type.GetConstructors();
            var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();

            var parameters = constructor.GetParameters()
                .Select(p => GetService(p.ParameterType))
                .ToArray();

            return Activator.CreateInstance(type, parameters);
        }
    }
}