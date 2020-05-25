﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions.Internal;

namespace NBB.Data.EntityFramework
{
    public class EfQuery<TEntity, TContext> : IQueryable<TEntity>, IAsyncEnumerable<TEntity>
        where TContext : DbContext where TEntity : class
    {
        private readonly IQueryable<TEntity> _q;

        public EfQuery(TContext c)
        {
            c.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _q = c.Set<TEntity>();
        }


        public Type ElementType => _q.ElementType;

        public Expression Expression => _q.Expression;

        public IQueryProvider Provider => _q.Provider;

        public IEnumerator<TEntity> GetEnumerator() => _q.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _q.GetEnumerator();

        IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetEnumerator() => _q.AsAsyncEnumerable().GetEnumerator();
    }
}
