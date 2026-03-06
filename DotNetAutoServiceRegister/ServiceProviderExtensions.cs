using Microsoft.Extensions.DependencyInjection;
using System;

namespace DotNetAutoServiceRegister
{
    public static class ServiceProviderExtensions
    {
        public static T? GetServiceByKey<T>(this IServiceProvider provider, string key) where T : class
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

#if NET8_0_OR_GREATER
            return provider.GetKeyedService<T>(key);
#else
            var registry = provider.GetService<KeyedServiceRegistry>();
            if (registry == null)
            {
                throw new InvalidOperationException(
                    "KeyedServiceRegistry not found. Did you call AddDecoratedServices()?");
            }
            return registry.Resolve<T>(key);
#endif
        }

        public static T GetRequiredServiceByKey<T>(this IServiceProvider provider, string key) where T : class
        {
            var service = provider.GetServiceByKey<T>(key);
            if (service == null)
            {
                throw new InvalidOperationException(
                    $"No service for type '{typeof(T).Name}' with key '{key}' has been registered.");
            }
            return service;
        }

        public static object? GetServiceByKey(this IServiceProvider provider, Type serviceType, string key)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

#if NET8_0_OR_GREATER
            if (provider is IKeyedServiceProvider keyedProvider)
            {
                return keyedProvider.GetKeyedService(serviceType, key);
            }
            return null;
#else
            var registry = provider.GetService<KeyedServiceRegistry>();
            if (registry == null)
            {
                throw new InvalidOperationException(
                    "KeyedServiceRegistry not found. Did you call AddDecoratedServices()?");
            }
            return registry.Resolve(serviceType, key);
#endif
        }
    }
}
