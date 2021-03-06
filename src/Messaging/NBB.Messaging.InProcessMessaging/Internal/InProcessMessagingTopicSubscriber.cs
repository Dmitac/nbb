﻿using Microsoft.Extensions.Logging;
using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.InProcessMessaging.Internal
{
    public class InProcessMessagingTopicSubscriber : IMessagingTopicSubscriber
    {
        private readonly IStorage _storage;
        private readonly ILogger<InProcessMessagingTopicSubscriber> _logger;
        private bool _subscribedToTopic;
        private readonly object lockObj = new object();

        public InProcessMessagingTopicSubscriber(IStorage storage, ILogger<InProcessMessagingTopicSubscriber> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public Task SubscribeAsync(string topic, Func<string, Task> handler, CancellationToken cancellationToken = default, MessagingSubscriberOptions options = null)
        {
            if (!_subscribedToTopic)
            {
                lock (lockObj)
                {
                    if (!_subscribedToTopic)
                    {
                        _subscribedToTopic = true;
                        return InternalSubscribeAsync(topic, handler, cancellationToken);
                    }
                }
            }

            return Task.CompletedTask;
        }

        private async Task InternalSubscribeAsync(string topic, Func<string, Task> handler, CancellationToken cancellationToken = default)
        {
            await _storage.AddSubscription(topic, async msg =>
            {
                try
                {
                    await handler(msg);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        "InProcessMessagingTopicSubscriber encountered an error when handling a message from topic {TopicName}.\n {Error}",
                        topic, ex);
                    //TODO: push to DLQ
                }
            }, cancellationToken);

            //_logger.LogInformation("InProcessMessagingTopicSubscriber has subscribed to topic {Topic}", topic);
        }

        public Task UnSubscribeAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
