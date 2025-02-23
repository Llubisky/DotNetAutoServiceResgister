namespace DotNetAutoServiceRegister
{
    public class DotNetAutoServiceRegister
    {
        /// <summary>
        /// Enum that defines the ServiceLifetime of the service we want to register
        /// </summary>
        public enum AutoServiceLifetime
        {
            Singleton,
            Scoped,
            Transient
        }

        /// <summary>
        /// Attribute that defines a class as a service. It will be registered by default as a Singleton
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        public class ServiceAttribute : Attribute
        {
            public AutoServiceLifetime Lifetime { get; }

            public ServiceAttribute(AutoServiceLifetime lifetime = AutoServiceLifetime.Singleton)
            {
                Lifetime = lifetime;
            }
        }

        /// <summary>
        /// Attribute that defines a class as a component. It will be registered by default as a Singleton
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        public class ComponentAttribute : Attribute
        {
            public AutoServiceLifetime Lifetime { get; }

            public ComponentAttribute(AutoServiceLifetime lifetime = AutoServiceLifetime.Singleton)
            {
                Lifetime = lifetime;
            }
        }
        /// <summary>
        /// Attribute that defines a class as a repository. It will be registered by defaul as transient
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        public class RepositoryAttribute : Attribute
        {
            public AutoServiceLifetime Lifetime { get; }

            public RepositoryAttribute(AutoServiceLifetime lifetime = AutoServiceLifetime.Transient)
            {
                Lifetime = lifetime;


            }
        }
    }
}