using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using static DotNetAutoServiceRegister.DotNetAutoServiceRegister;
using static DotNetAutoServiceRegister.ServiceCollectionExtensions;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;


namespace DotNetAutoServiceRegisterTests
{
    public class ServiceCollectionExtensionsTests
    {

        private class TestService : ITestService
        {
        }

        private interface ITestService
        {
        }

        [Theory]
        [InlineData(AutoServiceLifetime.Singleton)]
        [InlineData(AutoServiceLifetime.Scoped)]
        [InlineData(AutoServiceLifetime.Transient)]
        public void RegisterService_Should_Register_Service_With_Correct_Lifetime(AutoServiceLifetime lifetime)
        {
            // Arrange
            var services = new ServiceCollection();
            var type = typeof(TestService);

            // Act
            RegisterService(services, type, lifetime);

            // Assert
            var serviceProvider = services.BuildServiceProvider();

            switch (lifetime)
            {
                case AutoServiceLifetime.Singleton:
                    var singletonService1 = serviceProvider.GetService<ITestService>();
                    var singletonService2 = serviceProvider.GetService<ITestService>();
                    Assert.Same(singletonService1, singletonService2);
                    break;
                case AutoServiceLifetime.Scoped:
                    using (var scope1 = serviceProvider.CreateScope())
                    using (var scope2 = serviceProvider.CreateScope())
                    {
                        var scopedService1 = scope1.ServiceProvider.GetService<ITestService>();
                        var scopedService2 = scope2.ServiceProvider.GetService<ITestService>();
                        Assert.NotSame(scopedService1, scopedService2);
                    }
                    break;
                case AutoServiceLifetime.Transient:
                    var transientService1 = serviceProvider.GetService<ITestService>();
                    var transientService2 = serviceProvider.GetService<ITestService>();
                    Assert.NotSame(transientService1, transientService2);
                    break;
            }
        }
    }
}
