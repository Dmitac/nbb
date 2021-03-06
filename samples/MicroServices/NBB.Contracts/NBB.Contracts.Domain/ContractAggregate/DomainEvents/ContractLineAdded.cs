﻿using NBB.Domain;
using System;

namespace NBB.Contracts.Domain.ContractAggregate.DomainEvents
{
    public class ContractLineAdded : DomainEvent
    {
        public Guid ContractLineId { get; }

        public Guid ContractId { get; }

        public string Product { get; }

        public decimal Price { get; }

        public int Quantity { get; }

        public ContractLineAdded(Guid contractLineId, Guid contractId, string product, decimal price, int quantity, DomainEventMetadata metadata = null)
            : base(metadata)
        {
            ContractLineId = contractLineId;
            ContractId = contractId;
            Product = product;
            Price = price;
            Quantity = quantity;
        }
    }
}