namespace Neo.Sample.Entities;

[NeoCreate("Customers")]
public sealed class Customer
{
    public long Id { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }
}