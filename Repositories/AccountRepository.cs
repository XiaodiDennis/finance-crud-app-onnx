using System;
using System.Collections.Generic;
using FinanceCrudApp.Data;
using FinanceCrudApp.Models;

namespace FinanceCrudApp.Repositories;

public class AccountRepository
{
    public List<Account> GetAll()
    {
        var accounts = new List<Account>();

        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT AccountId, Name, AccountType, CurrentBalance, IsActive
FROM Accounts
ORDER BY AccountId;";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            accounts.Add(new Account
            {
                AccountId = reader.GetInt32(0),
                Name = reader.GetString(1),
                AccountType = reader.GetString(2),
                CurrentBalance = reader.GetDecimal(3),
                IsActive = reader.GetInt32(4) == 1
            });
        }

        return accounts;
    }

    public void Insert(Account account)
    {
        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Accounts (Name, AccountType, CurrentBalance, IsActive)
VALUES ($name, $type, $balance, $isActive);";

        command.Parameters.AddWithValue("$name", account.Name);
        command.Parameters.AddWithValue("$type", account.AccountType);
        command.Parameters.AddWithValue("$balance", account.CurrentBalance);
        command.Parameters.AddWithValue("$isActive", account.IsActive ? 1 : 0);

        command.ExecuteNonQuery();
    }

    public void Update(Account account)
    {
        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE Accounts
SET Name = $name,
    AccountType = $type,
    CurrentBalance = $balance,
    IsActive = $isActive
WHERE AccountId = $id;";

        command.Parameters.AddWithValue("$name", account.Name);
        command.Parameters.AddWithValue("$type", account.AccountType);
        command.Parameters.AddWithValue("$balance", account.CurrentBalance);
        command.Parameters.AddWithValue("$isActive", account.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("$id", account.AccountId);

        command.ExecuteNonQuery();
    }

    public void Delete(int accountId)
    {
        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
DELETE FROM Accounts
WHERE AccountId = $id;";

        command.Parameters.AddWithValue("$id", accountId);

        command.ExecuteNonQuery();
    }
}
