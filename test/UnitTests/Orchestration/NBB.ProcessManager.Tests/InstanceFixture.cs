﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NBB.Core.Effects;
using NBB.EventStore.InMemory;
using NBB.EventStore.Internal;
using NBB.ProcessManager.Runtime.Persistence;

namespace NBB.ProcessManager.Tests
{
    public class InstanceDataRepositoryFixture
    {
        public InstanceDataRepository Repository { get; private set; }

        public InstanceDataRepositoryFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole())
                .BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<EventStore.EventStore>>();
            var sideEffectMediator = Mock.Of<ISideEffectMediator>();
            var interpreter = new Interpreter(sideEffectMediator);

            Repository = new InstanceDataRepository(
                new EventStore.EventStore(new InMemoryRepository(), new NewtonsoftJsonEventStoreSerDes(), logger),
                interpreter);
        }
    }
}