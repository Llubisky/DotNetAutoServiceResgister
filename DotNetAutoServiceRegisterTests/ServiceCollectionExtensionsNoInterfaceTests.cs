using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static DotNetAutoServiceRegister.DotNetAutoServiceRegister;
using static DotNetAutoServiceRegister.ServiceCollectionExtensions;

namespace DotNetAutoServiceRegisterTests
{
    public class ServiceCollectionExtensionsNoInterfaceTests
    {
        private class NoInterfaceService { }

        [Theory]
        [InlineData(AutoServiceLifetime.Singleton)]
        [InlineData(AutoServiceLifetime.Scoped)]
        [InlineData(AutoServiceLifetime.Transient)]
        public void RegisterService_Should_Register_Type_Without_Interface(AutoServiceLifetime lifetime)
        {
            var services = new ServiceCollection();
            var type = typeof(NoInterfaceService);

            RegisterService(services, type, lifetime);
            var provider = services.BuildServiceProvider();

            switch (lifetime)
            {
                case AutoServiceLifetime.Singleton:
                    var s1 = provider.GetService<NoInterfaceService>();
                    var s2 = provider.GetService<NoInterfaceService>();
                    Assert.Same(s1, s2);
                    break;
                case AutoServiceLifetime.Scoped:
                    using (var scope1 = provider.CreateScope())
                    using (var scope2 = provider.CreateScope())
                    {
                        var sc1 = scope1.ServiceProvider.GetService<NoInterfaceService>();
                        var sc2 = scope2.ServiceProvider.GetService<NoInterfaceService>();
                        Assert.NotSame(sc1, sc2);
                    }
                    break;
                case AutoServiceLifetime.Transient:
                    var t1 = provider.GetService<NoInterfaceService>();
                    var t2 = provider.GetService<NoInterfaceService>();
                    Assert.NotSame(t1, t2);
                    break;
            }
        }
    }
}
