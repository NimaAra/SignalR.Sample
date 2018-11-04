namespace SignalR.Sample.Client
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http.Connections;
    using Microsoft.AspNetCore.SignalR.Client;
    using Microsoft.Extensions.Logging;

    internal static class Program
    {
        static async Task Main()
        {
            const string ENDPOINT = "http://localhost:5000/hubs/myHub";

            var conn = new HubConnectionBuilder()
                .ConfigureLogging(o => {
                    o.SetMinimumLevel(LogLevel.Debug);
                    o.AddConsole();
                })
                .WithUrl(ENDPOINT, o => o.Transports = HttpTransportType.LongPolling)
                .Build();
	
            conn.Closed += e => {
                Console.WriteLine("Ooops:\r\n " + e.Message);	
                return Task.CompletedTask;
            };

            conn.On<string>("OnSomeEvent", stuff => Console.WriteLine("Message:\r\n {0}", stuff));

            await conn.StartAsync();
            Console.ReadLine();
        }
    }
}
