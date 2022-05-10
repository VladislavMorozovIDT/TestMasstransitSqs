using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SqsReceiver
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            
            services.AddMassTransit(x =>
            {
                x.AddConsumer<Consumer>();
                x.UsingAmazonSqs((context, cfg) =>
                {
                    cfg.Host(new Uri("amazonsqs://localhost:4566"), h =>
                    {
                        h.AccessKey("X");
                        h.SecretKey("X");
                        
                        h.Config(new AmazonSQSConfig {ServiceURL = "http://localhost:4566"});
                        h.Config(new AmazonSimpleNotificationServiceConfig {ServiceURL = "http://localhost:4566"});
                    });
                    
                    cfg.ReceiveEndpoint("receiver", e =>
                    {
                        // disable the default topic binding
                        e.ConfigureConsumeTopology = false;
                        e.ConfigureConsumer<Consumer>(context);
                        
                        e.Subscribe("custom-topic", s =>
                        {
                            // set topic attributes
                            s.TopicAttributes["DisplayName"] = "Custom Topic";
                            s.TopicSubscriptionAttributes["FilterPolicy"] = "{\"ProviderId\": [\"my\"]}";
                            e.UseMessageRetry(r => r.Incremental(5, TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(100)));
                        });
                        e.Subscribe("MassTransit-Fault--shared-SmthHappenEvent--", s =>
                        {
                        });
                    });
                });
            });

            services.AddMassTransitHostedService();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Hello World!"); });
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}