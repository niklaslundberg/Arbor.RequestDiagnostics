using System;
using System.IO;
using System.Linq;
using System.Net;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.JsonConfiguration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace RemoteIPTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };

            string configFile = Environment.GetEnvironmentVariable("config");

            IKeyValueConfiguration keyValueConfiguration = NoConfiguration.Empty;

            if (!string.IsNullOrWhiteSpace(configFile) && File.Exists(configFile))
            {
                keyValueConfiguration = KeyValueConfigurationManager
                    .Add(new JsonKeyValueConfiguration(configFile, false)).Build();
            }

            string limitKey = "limit";
            string proxyKey = "proxy";

            Log.Logger.Information("Found values {Values}",
                string.Join(", ", keyValueConfiguration.AllValues.Select(s => s.ToString())));

            if (int.TryParse(keyValueConfiguration[limitKey], out int limit) && limit >= 0)
            {
                Log.Logger.Information("Adding limit {Limit}", limit);
                forwardedHeadersOptions.ForwardLimit = limit;
            }
            else
            {
                Log.Logger.Information("No limit added");
            }

            if (IPAddress.TryParse(keyValueConfiguration[proxyKey], out IPAddress address))
            {
                Log.Logger.Information("Adding proxy {Proxy}", address.ToString());
                forwardedHeadersOptions.KnownProxies.Add(address);
            }
            else
            {
                Log.Logger.Information("No proxy added");
            }

            app.UseForwardedHeaders(forwardedHeadersOptions);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
