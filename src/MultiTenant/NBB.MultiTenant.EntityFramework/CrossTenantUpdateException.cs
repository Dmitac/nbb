﻿using System;
using System.Collections.Generic;

namespace NBB.MultiTenant.EntityFramework
{
    public class CrossTenantUpdateException : ApplicationException
    {
        public IList<Guid> TenantIds { get; private set; }

        public CrossTenantUpdateException(IList<Guid> tenantIds)
        {
            TenantIds = tenantIds;
        }
    }
}