namespace Models.Domain
{
    public class DomainEnums
    {
        public enum TransactionTypes
        {
            None = 0,
            Insert = 1,
            Update = 2,
            Delete = 3,
            Archive = 4 // Soft delete / logical delete :)
        }
    }
}
