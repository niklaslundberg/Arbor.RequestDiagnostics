using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Core;

namespace RemoteIPTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Logger logger = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Verbose().CreateLogger();

            Log.Logger = logger;

            CreateWebHostBuilder(args, logger).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, Logger logger)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options =>
                {
                    if (!Debugger.IsAttached)
                    {
                        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                        foreach (NetworkInterface network in networkInterfaces)
                        {
                            IPInterfaceProperties properties = network.GetIPProperties();

                            foreach (UnicastIPAddressInformation address in properties.UnicastAddresses)
                            {
                                if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                                {
                                    continue;
                                }

                                if (IPAddress.IsLoopback(address.Address))
                                {
                                    continue;
                                }

                                options.Listen(address.Address, 5000);

                                break;
                            }
                        }
                    }
                })
                .UseSerilog(logger)
                .UseStartup<Startup>();
        }
    }
}
