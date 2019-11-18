﻿using NBB.Core.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore.Abstractions
{
    public class EventStoreUowDecorator<TEntity> : IUow<TEntity>
        where TEntity : IEventedEntity, IIdentifiedEntity
    {
        private readonly IUow<TEntity> _inner;
        private readonly IEventStore _eventStore;

        public EventStoreUowDecorator(IUow<TEntity> inner, IEventStore eventStore)
        {
            _inner = inner;
            _eventStore = eventStore;
        }

        public IEnumerable<TEntity> GetChanges()
        {
            return _inner.GetChanges();
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            var streams = this.GetChanges()
                .Select(e => new {Stream = e.GetStream(), Events = e.GetUncommittedChanges().ToList()}).ToList();
            
            await _inner.SaveChangesAsync(cancellationToken);
            await OnAfterSave(cancellationToken, streams);
        }

        private async Task OnAfterSave(CancellationToken cancellationToken, IEnumerable<dynamic> changes)
        {
            foreach (var @entity in changes)
            {
                await _eventStore.AppendEventsToStreamAsync(@entity.Stream, @entity.Events, null, cancellationToken);
            }
        }
    }
}
