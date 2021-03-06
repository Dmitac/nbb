﻿using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SqlStreamStore;

namespace NBB.SQLStreamStore.Migrations
{
    public class SqlStreamStoreMigrator
    {
        public async Task MigrateDatabaseToLatestVersion(CancellationToken cancellationToken = default)
        {
            var store = GetStore();
            await store.CreateSchema(cancellationToken);
        }

        public async Task EnsureDatabaseDeleted(CancellationToken cancellationToken = default)
        {
            var store = GetStore();
            try
            {
                await store.DropAll(cancellationToken);
            }
            catch
            {
                // will fail if the database does not exist
            }
        }

        private MsSqlStreamStore GetStore()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.Equals(environment, "development", StringComparison.OrdinalIgnoreCase);

            if (isDevelopment)
            {
                configurationBuilder.AddUserSecrets(Assembly.GetCallingAssembly());
            }

            var configuration = configurationBuilder.Build();
            var connectionString = configuration["EventStore:SqlStreamStore:ConnectionString"];
            var settings = new MsSqlStreamStoreSettings(connectionString);
            var store = new MsSqlStreamStore(settings);

            return store;
        }
    }
}
