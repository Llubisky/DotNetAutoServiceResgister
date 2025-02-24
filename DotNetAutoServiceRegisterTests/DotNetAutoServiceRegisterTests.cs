using static DotNetAutoServiceRegister.DotNetAutoServiceRegister;
namespace DotNetAutoServiceRegisterTests
{
    public class DotNetAutoServiceRegisterTests
    {
        [Fact]
        public void ServiceAttribute_Should_Have_Singleton_Lifetime_By_Default()
        {
            // Arrange & Act
            ServiceAttribute attribute = new ServiceAttribute();

            // Assert
            Assert.Equal(AutoServiceLifetime.Singleton, attribute.Lifetime);
        }

        [Theory]
        [InlineData(AutoServiceLifetime.Singleton)]
        [InlineData(AutoServiceLifetime.Scoped)]
        [InlineData(AutoServiceLifetime.Transient)]
        public void ServiceAttribute_Should_Set_Lifetime_Correctly(AutoServiceLifetime lifetime)
        {
            // Arrange & Act
            ServiceAttribute attribute = new ServiceAttribute(lifetime);

            // Assert
            Assert.Equal(lifetime, attribute.Lifetime);
        }

        [Theory]
        [InlineData(AutoServiceLifetime.Singleton)]
        [InlineData(AutoServiceLifetime.Scoped)]
        [InlineData(AutoServiceLifetime.Transient)]
        public void ComponentAttribute_Should_Set_Lifetime_Correctly(AutoServiceLifetime lifetime)
        {
            // Arrange & Act
            ComponentAttribute attribute = new ComponentAttribute(lifetime);

            // Assert
            Assert.Equal(lifetime, attribute.Lifetime);
        }

        [Theory]
        [InlineData(AutoServiceLifetime.Singleton)]
        [InlineData(AutoServiceLifetime.Scoped)]
        [InlineData(AutoServiceLifetime.Transient)]
        public void RepositoryAttribute_Should_Set_Lifetime_Correctly(AutoServiceLifetime lifetime)
        {
            // Arrange & Act
            RepositoryAttribute attribute = new RepositoryAttribute(lifetime);

            // Assert
            Assert.Equal(lifetime, attribute.Lifetime);
        }
    }
}