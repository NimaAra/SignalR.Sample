namespace SignalR.Sample.Server
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector;

    internal static class SimpleInjectorHelper
    {
        internal static void InitializeContainerForAspNet(this IApplicationBuilder builder, Container container)
        {
            container.AutoCrossWireAspNetComponents(builder);
            container.Verify();
        }

        internal static void IntegrateWithAspNet(this IServiceCollection services, Container container)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton(container);

            services.EnableSimpleInjectorCrossWiring(container);
            services.UseSimpleInjectorAspNetRequestScoping(container);
        }

        internal static void IntegrateWithSignalR(this IServiceCollection services) =>
            services.AddSingleton(typeof(IHubActivator<>), typeof(SimpleInjectorHubActivator<>));

        private sealed class SimpleInjectorHubActivator<T> : IHubActivator<T> where T : Hub
        {
            private readonly Container _container;

            public SimpleInjectorHubActivator(Container container) => _container = container;

            public T Create() => _container.GetInstance<T>();

            // SimpleInjector takes care of this.
            public void Release(T hub) {}
        }
    }
}
