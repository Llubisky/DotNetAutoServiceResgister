using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static DotNetAutoServiceRegister.DotNetAutoServiceRegister;
using static DotNetAutoServiceRegister.ServiceCollectionExtensions;
using DotNetAutoServiceRegister;

namespace DotNetAutoServiceRegisterTests
{
    public class ServiceCollectionExtensionsMultiKeyTests
    {
        [Service(AutoServiceLifetime.Singleton, "key1#key2#key3")]
        private class MultiKeyService : IMultiKeyService { }
        private interface IMultiKeyService { }

        [Fact]
        public void RegisterService_Should_Register_Service_With_Multiple_Keys()
        {
            var services = new ServiceCollection();
            var type = typeof(MultiKeyService);
            string keys = "key1#key2#key3";

            RegisterService(services, type, AutoServiceLifetime.Singleton, keys);
            var provider = services.BuildServiceProvider();

            var service1 = provider.GetServiceByKey<IMultiKeyService>("key1");
            var service2 = provider.GetServiceByKey<IMultiKeyService>("key2");
            var service3 = provider.GetServiceByKey<IMultiKeyService>("key3");

            Assert.NotNull(service1);
            Assert.NotNull(service2);
            Assert.NotNull(service3);

            Assert.Same(service1, service2);
            Assert.Same(service2, service3);
        }

        [Fact]
        public void AddDecoratedServices_Should_Register_MultiKey_Services()
        {
            var services = new ServiceCollection();
            var assembly = Assembly.GetExecutingAssembly();

            services.AddDecoratedServices(assembly);
            var provider = services.BuildServiceProvider();

            var service1 = provider.GetServiceByKey<IMultiKeyService>("key1");
            var service2 = provider.GetServiceByKey<IMultiKeyService>("key2");
            var service3 = provider.GetServiceByKey<IMultiKeyService>("key3");

            Assert.NotNull(service1);
            Assert.NotNull(service2);
            Assert.NotNull(service3);
            Assert.Same(service1, service2);
        }
    }
}
