using Microsoft.EntityFrameworkCore;
using Neo.Sample.Entities;

namespace Neo.Sample;

internal sealed class DbContext
{
    public DbSet<Customer> Customers { get; init; }
}