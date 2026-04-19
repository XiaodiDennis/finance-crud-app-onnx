using System;
using System.Collections.Generic;
using FinanceCrudApp.Data;
using FinanceCrudApp.Models;

namespace FinanceCrudApp.Repositories;

public class MerchantRepository
{
    public List<Merchant> GetAll()
    {
        var merchants = new List<Merchant>();

        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT MerchantId, Name, Description, IsActive
FROM Merchants
ORDER BY MerchantId;";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            merchants.Add(new Merchant
            {
                MerchantId = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                IsActive = reader.GetInt32(3) == 1
            });
        }

        return merchants;
    }

    public void Insert(Merchant merchant)
    {
        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Merchants (Name, Description, IsActive)
VALUES ($name, $description, $isActive);";

        command.Parameters.AddWithValue("$name", merchant.Name);
        command.Parameters.AddWithValue("$description", (object?)merchant.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("$isActive", merchant.IsActive ? 1 : 0);

        command.ExecuteNonQuery();
    }

    public void Update(Merchant merchant)
    {
        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE Merchants
SET Name = $name,
    Description = $description,
    IsActive = $isActive
WHERE MerchantId = $id;";

        command.Parameters.AddWithValue("$name", merchant.Name);
        command.Parameters.AddWithValue("$description", (object?)merchant.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("$isActive", merchant.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("$id", merchant.MerchantId);

        command.ExecuteNonQuery();
    }

    public void Delete(int merchantId)
    {
        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
DELETE FROM Merchants
WHERE MerchantId = $id;";

        command.Parameters.AddWithValue("$id", merchantId);

        command.ExecuteNonQuery();
    }
}
