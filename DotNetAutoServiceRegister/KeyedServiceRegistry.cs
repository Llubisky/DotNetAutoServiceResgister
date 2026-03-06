#if !NET8_0_OR_GREATER
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static DotNetAutoServiceRegister.DotNetAutoServiceRegister;

namespace DotNetAutoServiceRegister
{
    internal class KeyedServiceRegistrationStore
    {
        private readonly List<(Type ServiceType, string Key, Type ImplementationType, AutoServiceLifetime Lifetime, string GroupId)> _registrations = new();
        private int _groupCounter = 0;
        private string? _currentGroupId = null;

        public void StartGroup()
        {
            _currentGroupId = $"group_{++_groupCounter}";
        }

        public void EndGroup()
        {
            _currentGroupId = null;
        }

        public void Add(Type serviceType, string key, Type implementationType, AutoServiceLifetime lifetime)
        {
            var groupId = _currentGroupId ?? $"single_{++_groupCounter}";
            _registrations.Add((serviceType, key, implementationType, lifetime, groupId));
        }

        public IReadOnlyList<(Type ServiceType, string Key, Type ImplementationType, AutoServiceLifetime Lifetime, string GroupId)> GetRegistrations()
        {
            return _registrations;
        }
    }

    internal class KeyedServiceRegistry
    {
        private readonly ConcurrentDictionary<(Type ServiceType, string Key), ServiceRegistration> _registrations = new();
        private readonly ConcurrentDictionary<string, object> _sharedInstances = new();
        private readonly IServiceProvider _serviceProvider;

        public KeyedServiceRegistry(IServiceProvider serviceProvider, KeyedServiceRegistrationStore store)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            if (store != null)
            {
                foreach (var reg in store.GetRegistrations())
                {
                    RegisterType(reg.ServiceType, reg.Key, reg.ImplementationType, reg.Lifetime, reg.GroupId);
                }
            }
        }

        public void RegisterType(Type serviceType, string key, Type implementationType, AutoServiceLifetime lifetime, string groupId)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));

            var registration = new ServiceRegistration(implementationType, lifetime, groupId);
            _registrations[(serviceType, key)] = registration;
        }

        public object? Resolve(Type serviceType, string key)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            if (!_registrations.TryGetValue((serviceType, key), out var registration))
            {
                return null;
            }

            return ResolveInstance(registration);
        }

        private object ResolveInstance(ServiceRegistration registration)
        {
            switch (registration.Lifetime)
            {
                case AutoServiceLifetime.Singleton:
                case AutoServiceLifetime.Scoped:
                    return _sharedInstances.GetOrAdd(registration.GroupId, _ => CreateInstance(registration.ImplementationType));

                case AutoServiceLifetime.Transient:
                    return CreateInstance(registration.ImplementationType);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private object CreateInstance(Type implementationType)
        {
            var instance = ActivatorUtilities.CreateInstance(_serviceProvider, implementationType);
            if (instance != null)
            {
                return instance;
            }

            return Activator.CreateInstance(implementationType)!;
        }

        public T? Resolve<T>(string key) where T : class
        {
            return Resolve(typeof(T), key) as T;
        }

        public bool IsRegistered(Type serviceType, string key)
        {
            return _registrations.ContainsKey((serviceType, key));
        }

        private class ServiceRegistration
        {
            public Type ImplementationType { get; }
            public AutoServiceLifetime Lifetime { get; }
            public string GroupId { get; }

            public ServiceRegistration(Type implementationType, AutoServiceLifetime lifetime, string groupId)
            {
                ImplementationType = implementationType;
                Lifetime = lifetime;
                GroupId = groupId;
            }
        }
    }

    internal static class ActivatorUtilities
    {
        public static object CreateInstance(IServiceProvider provider, Type type)
        {
            var constructors = type.GetConstructors();

            foreach (var ctor in constructors.OrderByDescending(c => c.GetParameters().Length))
            {
                var parameters = ctor.GetParameters();
                var args = new object[parameters.Length];
                bool canCreate = true;

                for (int i = 0; i < parameters.Length; i++)
                {
                    var service = provider.GetService(parameters[i].ParameterType);
                    if (service == null && !parameters[i].HasDefaultValue)
                    {
                        canCreate = false;
                        break;
                    }
                    args[i] = service ?? parameters[i].DefaultValue!;
                }

                if (canCreate)
                {
                    return ctor.Invoke(args);
                }
            }

            return Activator.CreateInstance(type)!;
        }
    }
}
#endif
