﻿using System.IO;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBB.Application.DataContracts;
using NBB.Contracts.Application.CommandHandlers;
using NBB.Contracts.ReadModel.Data;
using NBB.Contracts.WriteModel.Data;
using NBB.Correlation.Serilog;
using NBB.Domain;
using NBB.EventStore;
using NBB.EventStore.AdoNet;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host;
using NBB.Messaging.Host.Builder;
using NBB.Messaging.Host.MessagingPipeline;
using NBB.Messaging.Kafka;
using NBB.Resiliency;
using Serilog;
using Serilog.Events;

namespace NBB.Contracts.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        public static async Task MainAsync(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureHostConfiguration(config =>
                {
                    config.AddEnvironmentVariables("NETCORE_");
                })
                .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                {
                    configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
                    configurationBuilder.AddJsonFile("appsettings.json", true);
                    configurationBuilder.AddEnvironmentVariables();

                    if (hostBuilderContext.HostingEnvironment.IsDevelopment())
                    {
                        configurationBuilder.AddUserSecrets<Program>();
                    }
                })
                .ConfigureLogging((hostingContext, loggingBuilder) =>
                {
                    var connectionString = hostingContext.Configuration.GetConnectionString("Logs");

                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .Enrich.With<CorrelationLogEventEnricher>()
                        .WriteTo.MSSqlServer(connectionString, "Logs", autoCreateSqlTable: true)
                        .CreateLogger();

                    loggingBuilder.AddSerilog(dispose: true);
                    loggingBuilder.AddFilter("Microsoft", logLevel => logLevel >= LogLevel.Warning);
                    loggingBuilder.AddConsole();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddMediatR(typeof(ContractCommandHandlers).Assembly);

                    services.AddKafkaMessaging();

                    services.AddContractsWriteModelDataAccess();
                    services.AddContractsReadModelDataAccess();


                    services.AddEventStore()
                        .WithNewtownsoftJsonEventStoreSeserializer(new[] {new SingleValueObjectConverter()})
                        .WithAdoNetEventRepository();

                    services.AddResiliency();

                    services.AddMessagingHost()
                        .AddSubscriberServices(config =>
                            config.FromMediatRHandledCommands().AddClassesAssignableTo<Command>())
                        .WithOptions(config => config.Options.ConsumerType = MessagingConsumerType.CompetingConsumer)
                        .UsePipeline(pipelineBuilder => pipelineBuilder
                            .UseCorrelationMiddleware()
                            .UseExceptionHandlingMiddleware()
                            .UseDefaultResiliencyMiddleware()
                            .UseMediatRMiddleware()
                        );
                });

            await builder.RunConsoleAsync();
        }
    }
}