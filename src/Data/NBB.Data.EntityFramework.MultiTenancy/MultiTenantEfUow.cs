﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.MultiTenancy.Abstractions.Context;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public class MultiTenantEfUow<TEntity, TContext> : IUow<TEntity>
        where TEntity : class
        where TContext : DbContext
    {
        private readonly TContext _dbContext;
        private readonly ILogger<MultiTenantEfUow<TEntity, TContext>> _logger;
        private readonly ITenantContextAccessor _tenantContextAccessor;

        public MultiTenantEfUow(TContext dbContext, ITenantContextAccessor tenantContextAccessor, ILogger<MultiTenantEfUow<TEntity, TContext>> logger)
        {
            _dbContext = dbContext;
            _tenantContextAccessor = tenantContextAccessor;
            _logger = logger;
        }

        public IEnumerable<TEntity> GetChanges()
        {
            return _dbContext.ChangeTracker.Entries<TEntity>().Select(ee => ee.Entity);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var tenantId = _tenantContextAccessor.TenantContext.GetTenantId();

            _dbContext.SetTenantId(tenantId);
            await _dbContext.SaveChangesAsync(cancellationToken);

            stopWatch.Stop();
            _logger.LogDebug("MultiTenantEfUow.SaveChangesAsync for {EntityType} took {ElapsedMilliseconds} ms", typeof(TEntity).Name, stopWatch.ElapsedMilliseconds);
        }
    }
}