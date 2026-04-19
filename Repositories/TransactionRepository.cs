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

    public List<SummaryItem> GetExpenseTotalsByCategory()
    {
        var items = new List<SummaryItem>();

        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT c.Name, SUM(t.Amount)
FROM Transactions t
JOIN Categories c ON t.CategoryId = c.CategoryId
WHERE t.TransactionType = 'Expense'
GROUP BY c.Name
ORDER BY SUM(t.Amount) DESC;";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            items.Add(new SummaryItem
            {
                Label = reader.GetString(0),
                Value = Convert.ToDecimal(reader.GetDouble(1))
            });
        }

        return items;
    }

    public List<SummaryItem> GetNetTotalsByMonth()
    {
        var items = new List<SummaryItem>();

        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
    substr(TransactionDate, 1, 7) AS YearMonth,
    SUM(CASE WHEN TransactionType = 'Income' THEN Amount ELSE -Amount END) AS NetAmount
FROM Transactions
GROUP BY substr(TransactionDate, 1, 7)
ORDER BY YearMonth;";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            items.Add(new SummaryItem
            {
                Label = reader.GetString(0),
                Value = Convert.ToDecimal(reader.GetDouble(1))
            });
        }

        return items;
    }

    public List<SummaryItem> GetDailyExpenseTotalsLast30Days()
    {
        var items = new List<SummaryItem>();

        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
    TransactionDate,
    IFNULL(SUM(Amount), 0) AS TotalExpense
FROM Transactions
WHERE TransactionType = 'Expense'
GROUP BY TransactionDate
ORDER BY TransactionDate DESC
LIMIT 30;";

        using var reader = command.ExecuteReader();

        var temp = new List<SummaryItem>();

        while (reader.Read())
        {
            temp.Add(new SummaryItem
            {
                Label = reader.GetString(0),
                Value = Convert.ToDecimal(reader.GetDouble(1))
            });
        }

        temp.Reverse();
        return temp;
    }

    public DashboardSummary GetDashboardSummary()
    {
        var summary = new DashboardSummary();

        using var connection = DbManager.GetConnection();
        connection.Open();

        summary.CategoryCount = ExecuteCount(connection, "SELECT COUNT(*) FROM Categories;");
        summary.MerchantCount = ExecuteCount(connection, "SELECT COUNT(*) FROM Merchants;");
        summary.AccountCount = ExecuteCount(connection, "SELECT COUNT(*) FROM Accounts;");
        summary.TransactionCount = ExecuteCount(connection, "SELECT COUNT(*) FROM Transactions;");

        summary.TotalIncome = ExecuteDecimal(connection,
            "SELECT IFNULL(SUM(Amount), 0) FROM Transactions WHERE TransactionType = 'Income';");

        summary.TotalExpense = ExecuteDecimal(connection,
            "SELECT IFNULL(SUM(Amount), 0) FROM Transactions WHERE TransactionType = 'Expense';");

        summary.NetBalance = summary.TotalIncome - summary.TotalExpense;

        return summary;
    }

    private int ExecuteCount(Microsoft.Data.Sqlite.SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt32(command.ExecuteScalar() ?? 0);
    }

    private decimal ExecuteDecimal(Microsoft.Data.Sqlite.SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        object? result = command.ExecuteScalar();

        if (result == null || result == DBNull.Value)
            return 0;

        return Convert.ToDecimal(result);
    }
}
