namespace SignalR.Sample.Server
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    internal static class Program
    {
        static void Main()
        {
            using (var host = new WebHostBuilder()
                .UseKestrel()
                .ConfigureServices(Startup.ConfigureServices)
                .Configure(Startup.Configure)
                .Build())
            {
                host.Run();
            }
        }
    }

    internal static class Startup
    {
        private static readonly Container _container;

        static Startup()
        {
            _container = new Container();
            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        }
        
        public static void ConfigureServices(IServiceCollection services)
        {
            _container.Register(typeof(MyHub), typeof(MyHub), Lifestyle.Scoped);
            
            services.AddSignalR(o => o.EnableDetailedErrors = true);
            services.IntegrateWithSignalR();
            services.IntegrateWithAspNet(_container);
        }

        public static void Configure(IApplicationBuilder app)
        {
            app.InitializeContainerForAspNet(_container);
            
            app.UseSignalR(r => r.MapHub<MyHub>("/hubs/myHub"));
        }
    }

    internal sealed class MyHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine("Connected");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("Disconnected due to:\r\n " + exception);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
