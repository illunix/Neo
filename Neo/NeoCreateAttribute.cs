namespace Neo;

[AttributeUsage(AttributeTargets.Class)]
public sealed class NeoCreateAttribute : Attribute
{
    public string TableName { get; private set; }

    public NeoCreateAttribute(string tableName)
        => TableName = tableName;
}