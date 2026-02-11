using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using static DotNetAutoServiceRegister.DotNetAutoServiceRegister;

namespace DotNetAutoServiceRegister
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Method that will add the decorators to your services
        /// </summary>
        /// <param name="services">IServiceCollection implementation</param>
        /// <param name="assembly">Assembly of the class</param>
        public static void AddDecoratedServices(this IServiceCollection services, Assembly assembly)
        {
            // Shame of me... I dont want to use var but here is the easiest and simplest way to do it
            var types = assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract)
                .Select(type => new
                {
                    Type = type,
                    ServiceAttribute = type.GetCustomAttribute<ServiceAttribute>(),
                    ComponentAttribute = type.GetCustomAttribute<ComponentAttribute>(),
                    RepositoryAttribute = type.GetCustomAttribute<RepositoryAttribute>()
                });
            // As each type in the collection is anonymous I will continue with my shame using var...
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
        /// <summary>
        /// Method that will register the service in the IServiceCollection
        /// </summary>
        /// <param name="services">IServiceCollection implementation</param>
        /// <param name="type">Type that we want to register</param>
        /// <param name="lifetime">LifeTime cicle for the service</param>
        public static void RegisterService(IServiceCollection services, Type type, AutoServiceLifetime lifetime, string? key = null)
        {
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
                var keys = key.Split('#', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var singleKey in keys)
                {
                    RegisterKeyedServicesTypes(services, type, lifetime, singleKey);
                }
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

        private static void RegisterInterfaces(IServiceCollection services, Type type, AutoServiceLifetime lifetime, string? key, Type[] interfaces)
        {
            foreach (Type? @interface in interfaces)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    var keys = key.Split('#', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (var singleKey in keys)
                    {
                        RegisterServicesKeyedInterfaces(services, type, lifetime, singleKey, @interface);
                    }
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
    }
}
