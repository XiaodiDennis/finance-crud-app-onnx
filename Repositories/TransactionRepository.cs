using System;
using System.Collections.Generic;
using FinanceCrudApp.Data;
using FinanceCrudApp.Models;

namespace FinanceCrudApp.Repositories;

public class TransactionRepository
{
    public List<FinanceTransaction> GetAll()
    {
        var transactions = new List<FinanceTransaction>();

        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
    t.TransactionId,
    t.TransactionDate,
    t.Amount,
    t.TransactionType,
    t.AccountId,
    a.Name,
    t.CategoryId,
    c.Name,
    t.MerchantId,
    m.Name,
    t.Note
FROM Transactions t
JOIN Accounts a ON t.AccountId = a.AccountId
JOIN Categories c ON t.CategoryId = c.CategoryId
JOIN Merchants m ON t.MerchantId = m.MerchantId
ORDER BY t.TransactionDate DESC, t.TransactionId DESC;";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            transactions.Add(new FinanceTransaction
            {
                TransactionId = reader.GetInt32(0),
                TransactionDate = DateTime.Parse(reader.GetString(1)),
                Amount = Convert.ToDecimal(reader.GetDouble(2)),
                TransactionType = reader.GetString(3),
                AccountId = reader.GetInt32(4),
                AccountName = reader.GetString(5),
                CategoryId = reader.GetInt32(6),
                CategoryName = reader.GetString(7),
                MerchantId = reader.GetInt32(8),
                MerchantName = reader.GetString(9),
                Note = reader.IsDBNull(10) ? null : reader.GetString(10)
            });
        }

        return transactions;
    }

    public void Insert(FinanceTransaction transaction)
    {
        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Transactions
(TransactionDate, Amount, TransactionType, AccountId, CategoryId, MerchantId, Note)
VALUES
($date, $amount, $type, $accountId, $categoryId, $merchantId, $note);";

        command.Parameters.AddWithValue("$date", transaction.TransactionDate.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$amount", transaction.Amount);
        command.Parameters.AddWithValue("$type", transaction.TransactionType);
        command.Parameters.AddWithValue("$accountId", transaction.AccountId);
        command.Parameters.AddWithValue("$categoryId", transaction.CategoryId);
        command.Parameters.AddWithValue("$merchantId", transaction.MerchantId);
        command.Parameters.AddWithValue("$note", (object?)transaction.Note ?? DBNull.Value);

        command.ExecuteNonQuery();
    }

    public void Update(FinanceTransaction transaction)
    {
        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE Transactions
SET TransactionDate = $date,
    Amount = $amount,
    TransactionType = $type,
    AccountId = $accountId,
    CategoryId = $categoryId,
    MerchantId = $merchantId,
    Note = $note
WHERE TransactionId = $id;";

        command.Parameters.AddWithValue("$date", transaction.TransactionDate.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$amount", transaction.Amount);
        command.Parameters.AddWithValue("$type", transaction.TransactionType);
        command.Parameters.AddWithValue("$accountId", transaction.AccountId);
        command.Parameters.AddWithValue("$categoryId", transaction.CategoryId);
        command.Parameters.AddWithValue("$merchantId", transaction.MerchantId);
        command.Parameters.AddWithValue("$note", (object?)transaction.Note ?? DBNull.Value);
        command.Parameters.AddWithValue("$id", transaction.TransactionId);

        command.ExecuteNonQuery();
    }

    public void Delete(int transactionId)
    {
        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
DELETE FROM Transactions
WHERE TransactionId = $id;";

        command.Parameters.AddWithValue("$id", transactionId);

        command.ExecuteNonQuery();
    }
}
