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
                    RegisterService(services, typeInfo.Type, typeInfo.ServiceAttribute.Lifetime);
                }
                else if (typeInfo.ComponentAttribute != null)
                {
                    RegisterService(services, typeInfo.Type, typeInfo.ComponentAttribute.Lifetime);
                }
                else if (typeInfo.RepositoryAttribute != null)
                {
                    RegisterService(services, typeInfo.Type, typeInfo.RepositoryAttribute.Lifetime);
                }
            }
        }
        /// <summary>
        /// Method that will register the service in the IServiceCollection
        /// </summary>
        /// <param name="services">IServiceCollection implementation</param>
        /// <param name="type">Type that we want to register</param>
        /// <param name="lifetime">LifeTime cicle for the service</param>
        public static void RegisterService(IServiceCollection services, Type type, AutoServiceLifetime lifetime)
        {
            Type[]? interfaces = type.GetInterfaces();

            foreach (Type? @interface in interfaces)
            {
                switch (lifetime)
                {
                    case DotNetAutoServiceRegister.AutoServiceLifetime.Singleton:
                        services.AddSingleton(@interface, type);
                        break;
                    case DotNetAutoServiceRegister.AutoServiceLifetime.Scoped:
                        services.AddScoped(@interface, type);
                        break;
                    case DotNetAutoServiceRegister.AutoServiceLifetime.Transient:
                        services.AddTransient(@interface, type);
                        break;
                }
            }
        }
    }
}
