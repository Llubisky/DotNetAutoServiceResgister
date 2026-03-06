using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static DotNetAutoServiceRegister.DotNetAutoServiceRegister;
using static DotNetAutoServiceRegister.ServiceCollectionExtensions;
using DotNetAutoServiceRegister; // For GetServiceByKey extension methods

namespace DotNetAutoServiceRegisterTests
{
    public class ServiceCollectionExtensionsKeyedTests
    {
        private class KeyedTestService : IKeyedTestService { }
        private interface IKeyedTestService { }

        [Theory]
        [InlineData(AutoServiceLifetime.Singleton)]
        [InlineData(AutoServiceLifetime.Transient)]
        public void RegisterService_Should_Register_Keyed_Service_With_Correct_Lifetime(AutoServiceLifetime lifetime)
        {
            var services = new ServiceCollection();
            var type = typeof(KeyedTestService);
            string key = "my-key";

            RegisterService(services, type, lifetime, key);
            var provider = services.BuildServiceProvider();

            switch (lifetime)
            {
                case AutoServiceLifetime.Singleton:
                    var s1 = provider.GetServiceByKey<IKeyedTestService>(key);
                    var s2 = provider.GetServiceByKey<IKeyedTestService>(key);
                    Assert.Same(s1, s2);
                    break;
                case AutoServiceLifetime.Transient:
                    var t1 = provider.GetServiceByKey<IKeyedTestService>(key);
                    var t2 = provider.GetServiceByKey<IKeyedTestService>(key);
                    Assert.NotSame(t1, t2);
                    break;
            }
        }

#if NET8_0_OR_GREATER
        [Fact]
        public void RegisterService_Should_Register_Keyed_Scoped_Service_Correctly()
        {
            var services = new ServiceCollection();
            var type = typeof(KeyedTestService);
            string key = "my-key";

            RegisterService(services, type, AutoServiceLifetime.Scoped, key);
            var provider = services.BuildServiceProvider();

            using (var scope1 = provider.CreateScope())
            using (var scope2 = provider.CreateScope())
            {
                var sc1 = scope1.ServiceProvider.GetServiceByKey<IKeyedTestService>(key);
                var sc2 = scope2.ServiceProvider.GetServiceByKey<IKeyedTestService>(key);
                Assert.NotSame(sc1, sc2);
            }
        }
#endif
    }
}
