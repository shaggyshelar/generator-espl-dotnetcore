using System;

namespace Models.Domain
{
    // Apply this interface to history models
    // Example: CustomerHistory
    public interface IHasHistory
    {
        DomainEnums.TransactionTypes TransactionType { get; set; }
        DateTime ChangedOn { get; set; }
        string ChangedBy { get; set; }
    }

    // Add this interface to any model that has an associated history table
    // Example: Customer
    public interface IStoreHistory<TFrom, TTo>
    {
        // This is used by the CrudService to generically get the history model to insert
        TTo Convert(
            TFrom model, 
            string applicationUserId,
            DomainEnums.TransactionTypes tranactionType);
    }
}
