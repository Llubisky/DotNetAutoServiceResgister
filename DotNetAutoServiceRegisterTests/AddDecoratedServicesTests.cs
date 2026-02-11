using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static DotNetAutoServiceRegister.DotNetAutoServiceRegister;
using static DotNetAutoServiceRegister.ServiceCollectionExtensions;

namespace DotNetAutoServiceRegisterTests
{
    public class AddDecoratedServicesTests
    {
        [Service]
        private class DecoratedService : IDecoratedService { }
        private interface IDecoratedService { }

        [Component(AutoServiceLifetime.Scoped)]
        private class DecoratedComponent : IDecoratedComponent { }
        private interface IDecoratedComponent { }

        [Repository(AutoServiceLifetime.Transient)]
        private class DecoratedRepository : IDecoratedRepository { }
        private interface IDecoratedRepository { }

        [Fact]
        public void AddDecoratedServices_Registers_All_Attributed_Classes()
        {
            var services = new ServiceCollection();
            var assembly = typeof(AddDecoratedServicesTests).Assembly;
            services.AddDecoratedServices(assembly);
            var provider = services.BuildServiceProvider();

            // Service: Singleton
            var s1 = provider.GetService<IDecoratedService>();
            var s2 = provider.GetService<IDecoratedService>();
            Assert.Same(s1, s2);

            // Component: Scoped
            using (var scope1 = provider.CreateScope())
            using (var scope2 = provider.CreateScope())
            {
                var c1 = scope1.ServiceProvider.GetService<IDecoratedComponent>();
                var c2 = scope2.ServiceProvider.GetService<IDecoratedComponent>();
                Assert.NotSame(c1, c2);
            }

            // Repository: Transient
            var r1 = provider.GetService<IDecoratedRepository>();
            var r2 = provider.GetService<IDecoratedRepository>();
            Assert.NotSame(r1, r2);
        }
    }
}
