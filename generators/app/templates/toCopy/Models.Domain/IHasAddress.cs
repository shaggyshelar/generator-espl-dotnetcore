namespace Models.Domain
{
    public interface IHasAddress
    {
        string Attn { get; set; }
        string StreetAddress1 { get; set; }
        string StreetAddress2 { get; set; }
        string City { get; set; }
        string State { get; set; }
        string ZipCode { get; set; }
    }
}
