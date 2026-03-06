using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using static DotNetAutoServiceRegister.DotNetAutoServiceRegister;

namespace DotNetAutoServiceRegister
{
    public static class ServiceCollectionExtensions
    {
#if !NET8_0_OR_GREATER
        internal static void EnsureKeyedServiceRegistry(IServiceCollection services)
        {
            bool storeRegistered = services.Any(sd => sd.ServiceType == typeof(KeyedServiceRegistrationStore));
            if (!storeRegistered)
            {
                services.AddSingleton<KeyedServiceRegistrationStore>();
            }

            bool registryRegistered = services.Any(sd => sd.ServiceType == typeof(KeyedServiceRegistry));
            if (!registryRegistered)
            {
                services.AddSingleton<KeyedServiceRegistry>(sp =>
                    new KeyedServiceRegistry(sp, sp.GetService<KeyedServiceRegistrationStore>()!));
            }
        }

        private static KeyedServiceRegistrationStore GetOrCreateStore(IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(KeyedServiceRegistrationStore));

            if (descriptor?.ImplementationInstance is KeyedServiceRegistrationStore existingStore)
            {
                return existingStore;
            }

            var store = new KeyedServiceRegistrationStore();

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            services.AddSingleton(store);

            return store;
        }
#endif

        public static void AddDecoratedServices(this IServiceCollection services, Assembly assembly)
        {
#if !NET8_0_OR_GREATER
            EnsureKeyedServiceRegistry(services);
#endif

            var types = assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract)
                .Select(type => new
                {
                    Type = type,
                    ServiceAttribute = type.GetCustomAttribute<ServiceAttribute>(),
                    ComponentAttribute = type.GetCustomAttribute<ComponentAttribute>(),
                    RepositoryAttribute = type.GetCustomAttribute<RepositoryAttribute>()
                });

            foreach (var typeInfo in types)
            {
                if (typeInfo.ServiceAttribute != null)
                {
                    RegisterService(services, typeInfo.Type, typeInfo.ServiceAttribute.Lifetime, (typeInfo.ServiceAttribute as dynamic)?.Key);
                }
                else if (typeInfo.ComponentAttribute != null)
                {
                    RegisterService(services, typeInfo.Type, typeInfo.ComponentAttribute.Lifetime, (typeInfo.ComponentAttribute as dynamic)?.Key);
                }
                else if (typeInfo.RepositoryAttribute != null)
                {
                    RegisterService(services, typeInfo.Type, typeInfo.RepositoryAttribute.Lifetime, (typeInfo.RepositoryAttribute as dynamic)?.Key);
                }
            }
        }

        public static void RegisterService(IServiceCollection services, Type type, AutoServiceLifetime lifetime, string? key = null)
        {
#if !NET8_0_OR_GREATER
            if (!string.IsNullOrEmpty(key))
            {
                EnsureKeyedServiceRegistry(services);
            }
#endif

            Type[]? interfaces = type.GetInterfaces();
            RegisterInterfaces(services, type, lifetime, key, interfaces);

            if (interfaces.Length == 0)
            {
                RegisterTypes(services, type, lifetime, key);
            }
        }

        private static void RegisterTypes(IServiceCollection services, Type type, AutoServiceLifetime lifetime, string? key)
        {
            if (!string.IsNullOrEmpty(key))
            {
#if NET8_0_OR_GREATER
                var keys = SplitAndTrimKeys(key);
                if (keys.Length > 1 && lifetime == AutoServiceLifetime.Singleton)
                {
                    RegisterMultiKeyServicesTypes(services, type, keys);
                }
                else
                {
                    foreach (var singleKey in keys)
                    {
                        RegisterKeyedServicesTypes(services, type, lifetime, singleKey);
                    }
                }
#else
                var keys = SplitAndTrimKeys(key);
                var store = GetOrCreateStore(services);

                if (keys.Length > 1)
                {
                    store.StartGroup();
                }

                foreach (var singleKey in keys)
                {
                    RegisterKeyedServicesTypesLegacy(services, type, lifetime, singleKey);
                }

                if (keys.Length > 1)
                {
                    store.EndGroup();
                }
#endif
            }
            else
            {
                RegisterServiceTypes(services, type, lifetime);
            }
        }

        private static void RegisterServiceTypes(IServiceCollection services, Type type, AutoServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case AutoServiceLifetime.Singleton:
                    services.AddSingleton(type);
                    break;
                case AutoServiceLifetime.Scoped:
                    services.AddScoped(type);
                    break;
                case AutoServiceLifetime.Transient:
                    services.AddTransient(type);
                    break;
            }
        }

#if NET8_0_OR_GREATER
        private static void RegisterKeyedServicesTypes(IServiceCollection services, Type type, AutoServiceLifetime lifetime, string key)
        {
            switch (lifetime)
            {
                case AutoServiceLifetime.Singleton:
                    services.AddKeyedSingleton(type, key, type);
                    break;
                case AutoServiceLifetime.Scoped:
                    services.AddKeyedScoped(type, key, type);
                    break;
                case AutoServiceLifetime.Transient:
                    services.AddKeyedTransient(type, key, type);
                    break;
            }
        }

        private static void RegisterMultiKeyServicesTypes(IServiceCollection services, Type type, string[] keys)
        {
            object? sharedInstance = null;
            var lockObj = new object();

            foreach (var key in keys)
            {
                services.AddKeyedSingleton(type, key, (sp, _) =>
                {
                    lock (lockObj)
                    {
                        if (sharedInstance == null)
                        {
                            sharedInstance = Activator.CreateInstance(type)!;
                        }
                        return sharedInstance;
                    }
                });
            }
        }
#endif

#if !NET8_0_OR_GREATER
        private static void RegisterKeyedServicesTypesLegacy(IServiceCollection services, Type type, AutoServiceLifetime lifetime, string key)
        {
            var store = GetOrCreateStore(services);
            store.Add(type, key, type, lifetime);
        }
#endif

        private static void RegisterInterfaces(IServiceCollection services, Type type, AutoServiceLifetime lifetime, string? key, Type[] interfaces)
        {
            foreach (Type? @interface in interfaces)
            {
                if (!string.IsNullOrEmpty(key))
                {
#if NET8_0_OR_GREATER
                    var keys = SplitAndTrimKeys(key);
                    if (keys.Length > 1 && lifetime == AutoServiceLifetime.Singleton)
                    {
                        RegisterMultiKeyServicesInterfaces(services, type, keys, @interface);
                    }
                    else
                    {
                        foreach (var singleKey in keys)
                        {
                            RegisterServicesKeyedInterfaces(services, type, lifetime, singleKey, @interface);
                        }
                    }
#else
                    var keys = SplitAndTrimKeys(key);
                    var store = GetOrCreateStore(services);

                    if (keys.Length > 1)
                    {
                        store.StartGroup();
                    }

                    foreach (var singleKey in keys)
                    {
                        RegisterServicesKeyedInterfacesLegacy(services, type, lifetime, singleKey, @interface);
                    }

                    if (keys.Length > 1)
                    {
                        store.EndGroup();
                    }
#endif
                }
                else
                {
                    RegisterServicesInterfaces(services, type, lifetime, @interface);
                }
            }
        }

        private static void RegisterServicesInterfaces(IServiceCollection services, Type type, AutoServiceLifetime lifetime, Type @interface)
        {
            switch (lifetime)
            {
                case AutoServiceLifetime.Singleton:
                    services.AddSingleton(@interface, type);
                    break;
                case AutoServiceLifetime.Scoped:
                    services.AddScoped(@interface, type);
                    break;
                case AutoServiceLifetime.Transient:
                    services.AddTransient(@interface, type);
                    break;
            }
        }

#if NET8_0_OR_GREATER
        private static void RegisterServicesKeyedInterfaces(IServiceCollection services, Type type, AutoServiceLifetime lifetime, string key, Type @interface)
        {
            switch (lifetime)
            {
                case AutoServiceLifetime.Singleton:
                    services.AddKeyedSingleton(@interface, key, type);
                    break;
                case AutoServiceLifetime.Scoped:
                    services.AddKeyedScoped(@interface, key, type);
                    break;
                case AutoServiceLifetime.Transient:
                    services.AddKeyedTransient(@interface, key, type);
                    break;
            }
        }

        private static void RegisterMultiKeyServicesInterfaces(IServiceCollection services, Type type, string[] keys, Type @interface)
        {
            object? sharedInstance = null;
            var lockObj = new object();

            foreach (var key in keys)
            {
                services.AddKeyedSingleton(@interface, key, (sp, _) =>
                {
                    lock (lockObj)
                    {
                        if (sharedInstance == null)
                        {
                            sharedInstance = Activator.CreateInstance(type)!;
                        }
                        return sharedInstance;
                    }
                });
            }
        }
#endif

#if !NET8_0_OR_GREATER
        private static void RegisterServicesKeyedInterfacesLegacy(IServiceCollection services, Type type, AutoServiceLifetime lifetime, string key, Type @interface)
        {
            var store = GetOrCreateStore(services);
            store.Add(@interface, key, type, lifetime);
        }
#endif

        private static string[] SplitAndTrimKeys(string key)
        {
#if NET5_0_OR_GREATER
            return key.Split('#', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
#else
            var keys = key.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = keys[i].Trim();
            }
            return keys;
#endif
        }
    }
}
