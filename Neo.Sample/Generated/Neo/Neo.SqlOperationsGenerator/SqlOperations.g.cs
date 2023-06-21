using Neo.Abstractions;
using Npgsql;

public static class SqlOperations
{
	public static async Task Add(this NpgsqlConnection conn, Neo.Sample.Entities.Customer e)
	{
		await using var cmd = new NpgsqlCommand("INSERT INTO Customers (Id, Name, Email) VALUES (@Id, @Name, @Email)", conn);

		cmd.Parameters.AddWithValue("Id", e.Id);
		cmd.Parameters.AddWithValue("Name", e.Name);
		cmd.Parameters.AddWithValue("Email", e.Email);

		await cmd.ExecuteNonQueryAsync();
	}
}
