using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Neo.Analysis;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Neo;

[Generator]
internal sealed class SqlOperationsGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext ctx)
        => ctx.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

    public void Execute(GeneratorExecutionContext ctx)
    {
        Debugger.Launch();

        if (ctx.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
            return;

        var sb = new StringBuilder();

        var classes = syntaxReceiver.CandidateClasses
            .Select(
                classSyntax =>
                    (INamedTypeSymbol)ctx.Compilation.GetSemanticModel(classSyntax.SyntaxTree)
                        .GetDeclaredSymbol(classSyntax)!
            ).TakeWhile(@class => @class is not null).Where(
                @class =>
                    @class.GetAttributes().Any(q => q.AttributeClass?.Name == nameof(NeoCreateAttribute))
            ).ToList();

        sb.AppendLine("using Neo.Abstractions;");
        sb.AppendLine("using Npgsql;");

        sb.AppendLine("\npublic static class SqlOperations");
        sb.AppendLine("{");

        foreach (var @class in classes)
        {
            var elo = CreateAddOperation(@class);
            sb.Append(elo);
        }

        sb.AppendLine("\n}");

        ctx.AddSource(
            "SqlOperations.g.cs",
            SourceText.From(
                sb.ToString(),
                Encoding.UTF8
            )
        );
    }

    private string CreateAddOperation(INamedTypeSymbol @class)
    {
        var sb = new StringBuilder();

        var propertyNames = GetClassPropertyNames(@class);

        sb.Append($"\tpublic static async Task Add(this NpgsqlConnection conn, {@class} e)");
        sb.Append("\n\t{");
        sb.Append($"\n\t\tawait using var cmd = new NpgsqlCommand(\"INSERT INTO {GetEntityTableName(@class)} ({string.Join(", ", GetClassPropertyNames(@class))}) VALUES ({string.Join(", ", GetClassPropertyNamesAsValues(@class))})\", conn);\n\n");

        foreach (var propertyName in propertyNames)
        {
            sb.AppendLine($"\t\tcmd.Parameters.AddWithValue(\"{propertyName}\", e.{propertyName});");
        }

        sb.Append("\n\t\tawait cmd.ExecuteNonQueryAsync();");
        sb.Append("\n\t}");

        return sb.ToString();
    }

    private string GetEntityTableName(INamedTypeSymbol @class)
    {
        var tableName = @class
            .GetAttributes()
            .FirstOrDefault()
            .ConstructorArguments
            .ElementAt(0)
            .Value;

        return tableName?.ToString()!;
    }

    private IEnumerable<string> GetClassPropertyNames(INamedTypeSymbol @class)
    {
        return @class
            .GetMembers()
            .Select(q => q.Name)
            .Where(q => 
                !q.Contains("k__BackingField") && 
                !q.Contains("get_") &&
                !q.Contains("set_") &&
                !q.Contains(".ctor")
            )
            .ToList();
    }

    private IEnumerable<string> GetClassPropertyNamesAsValues(INamedTypeSymbol @class)
    {
        return @class
            .GetMembers()
            .Select(q => $"@{q.Name}")
            .Where(q =>
                !q.Contains("k__BackingField") &&
                !q.Contains("get_") &&
                !q.Contains("set_") &&
                !q.Contains(".ctor")
            )
            .ToList();
    }
}