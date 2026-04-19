using FinanceCrudApp.Data;
using FinanceCrudApp.Models;

namespace FinanceCrudApp.Repositories;

public class UserRepository
{
    public User? GetByUsername(string username)
    {
        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT UserId, Username, PasswordHash, Role
FROM Users
WHERE Username = $username
LIMIT 1;";

        command.Parameters.AddWithValue("$username", username);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return null;

        return new User
        {
            UserId = reader.GetInt32(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            Role = reader.GetString(3)
        };
    }
}
