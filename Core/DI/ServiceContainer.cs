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
        private readonly HashSet<Type> _resolvingTypes = new(); // 用于循环依赖检测

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
                if (_singletonInstances.TryGetValue(serviceType, out var instance))
                    return instance;
                throw new ServiceNotFoundException($"服务未注册: {serviceType.Name}");
            }

            if (descriptor.Lifetime == ServiceLifetime.Singleton)
            {
                if (_singletonInstances.TryGetValue(serviceType, out var instance))
                    return instance;

                var singletonInstance = CreateInstance(descriptor.ImplementationType);
                _singletonInstances[serviceType] = singletonInstance;
                return singletonInstance;
            }

            return CreateInstance(descriptor.ImplementationType);
        }

        public IEnumerable<ServiceDescriptor> GetAllServiceDescriptors()
        {
            return _services.Values;
        }

        private object CreateInstance(Type type)
        {
            if (_resolvingTypes.Contains(type))
                throw new InvalidOperationException($"检测到循环依赖: {type.Name}");

            _resolvingTypes.Add(type);

            try
            {
                // 优化：可以缓存构造函数信息以提高性能，但对于tModLoader的场景，这通常不是瓶颈
                var constructor = type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
                if (constructor == null)
                    throw new InvalidOperationException($"类型 {type.Name} 没有公共构造函数。");

                var parameters = constructor.GetParameters()
                    .Select(p => GetService(p.ParameterType))
                    .ToArray();

                return Activator.CreateInstance(type, parameters);
            }
            finally
            {
                _resolvingTypes.Remove(type);
            }
        }
    }
}