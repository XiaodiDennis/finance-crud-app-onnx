using System;
using System.IO;
using FinanceCrudApp.Helpers;
using Microsoft.Data.Sqlite;

namespace FinanceCrudApp.Data;

public static class DbManager
{
    private static readonly string DbPath =
        Path.Combine(AppContext.BaseDirectory, "financecrud.db");

    private static readonly string ConnectionString =
        $"Data Source={DbPath}";

    public static SqliteConnection GetConnection()
    {
        return new SqliteConnection(ConnectionString);
    }

    public static void InitializeDatabase()
    {
        using var connection = GetConnection();
        connection.Open();

        CreateTables(connection);
        SeedDefaultUsers(connection);
        SeedDefaultCategories(connection);
        SeedDefaultMerchants(connection);
        SeedDefaultAccounts(connection);
        SeedDefaultTransactions(connection);
    }

    private static void CreateTables(SqliteConnection connection)
    {
        string sql = @"
CREATE TABLE IF NOT EXISTS Users (
    UserId INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Role TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Categories (
    CategoryId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    CategoryType TEXT NOT NULL,
    Description TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Merchants (
    MerchantId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    Description TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Accounts (
    AccountId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    AccountType TEXT NOT NULL,
    CurrentBalance REAL NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Transactions (
    TransactionId INTEGER PRIMARY KEY AUTOINCREMENT,
    TransactionDate TEXT NOT NULL,
    Amount REAL NOT NULL,
    TransactionType TEXT NOT NULL,
    AccountId INTEGER NOT NULL,
    CategoryId INTEGER NOT NULL,
    MerchantId INTEGER NOT NULL,
    Note TEXT,
    FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),
    FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    FOREIGN KEY (MerchantId) REFERENCES Merchants(MerchantId)
);
";
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static void SeedDefaultUsers(SqliteConnection connection)
    {
        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Users;";
        long count = (long)(countCommand.ExecuteScalar() ?? 0);
        if (count > 0) return;

        InsertUser(connection, "admin", "admin123", "admin");
        InsertUser(connection, "viewer", "viewer123", "viewer");
    }

    private static void InsertUser(SqliteConnection connection, string username, string password, string role)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Users (Username, PasswordHash, Role)
VALUES ($username, $passwordHash, $role);";

        command.Parameters.AddWithValue("$username", username);
        command.Parameters.AddWithValue("$passwordHash", PasswordHelper.HashPassword(password));
        command.Parameters.AddWithValue("$role", role);
        command.ExecuteNonQuery();
    }

    private static void SeedDefaultCategories(SqliteConnection connection)
    {
        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Categories;";
        long count = (long)(countCommand.ExecuteScalar() ?? 0);
        if (count > 0) return;

        InsertCategory(connection, "Food", "Expense", "Daily food and groceries");
        InsertCategory(connection, "Transport", "Expense", "Taxi, bus, metro, fuel");
        InsertCategory(connection, "Salary", "Income", "Main salary income");
    }

    private static void InsertCategory(SqliteConnection connection, string name, string type, string? description)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Categories (Name, CategoryType, Description, IsActive)
VALUES ($name, $type, $description, 1);";

        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$type", type);
        command.Parameters.AddWithValue("$description", (object?)description ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    private static void SeedDefaultMerchants(SqliteConnection connection)
    {
        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Merchants;";
        long count = (long)(countCommand.ExecuteScalar() ?? 0);
        if (count > 0) return;

        InsertMerchant(connection, "ATB Market", "Grocery chain");
        InsertMerchant(connection, "Uber", "Taxi and rides");
        InsertMerchant(connection, "Employer", "Salary source");
    }

    private static void InsertMerchant(SqliteConnection connection, string name, string? description)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Merchants (Name, Description, IsActive)
VALUES ($name, $description, 1);";

        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$description", (object?)description ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    private static void SeedDefaultAccounts(SqliteConnection connection)
    {
        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Accounts;";
        long count = (long)(countCommand.ExecuteScalar() ?? 0);
        if (count > 0) return;

        InsertAccount(connection, "Cash Wallet", "Cash", 5000);
        InsertAccount(connection, "Monobank Card", "Bank Card", 12000);
        InsertAccount(connection, "Savings", "Savings", 30000);
    }

    private static void InsertAccount(SqliteConnection connection, string name, string accountType, decimal balance)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Accounts (Name, AccountType, CurrentBalance, IsActive)
VALUES ($name, $type, $balance, 1);";

        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$type", accountType);
        command.Parameters.AddWithValue("$balance", balance);
        command.ExecuteNonQuery();
    }

    private static void SeedDefaultTransactions(SqliteConnection connection)
    {
        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Transactions;";
        long count = (long)(countCommand.ExecuteScalar() ?? 0);
        if (count > 0) return;

        var random = new Random(42);

        for (int month = 1; month <= 8; month++)
        {
            // salary once per month
            InsertTransaction(
                connection,
                new DateTime(2026, month, 5).ToString("yyyy-MM-dd"),
                28000 + month * 500,
                "Income",
                2,
                3,
                3,
                $"Salary for {month:00}/2026");

            // about 25 expense entries per month
            for (int i = 0; i < 25; i++)
            {
                int day = random.Next(1, 28);
                bool isFood = random.NextDouble() < 0.7;

                if (isFood)
                {
                    decimal amount = random.Next(150, 1600);
                    int accountId = random.NextDouble() < 0.6 ? 1 : 2;

                    InsertTransaction(
                        connection,
                        new DateTime(2026, month, day).ToString("yyyy-MM-dd"),
                        amount,
                        "Expense",
                        accountId,
                        1,
                        1,
                        "Seeded food expense");
                }
                else
                {
                    decimal amount = random.Next(80, 700);
                    int accountId = random.NextDouble() < 0.7 ? 1 : 2;

                    InsertTransaction(
                        connection,
                        new DateTime(2026, month, day).ToString("yyyy-MM-dd"),
                        amount,
                        "Expense",
                        accountId,
                        2,
                        2,
                        "Seeded transport expense");
                }
            }
        }
    }

    private static void InsertTransaction(
        SqliteConnection connection,
        string date,
        decimal amount,
        string type,
        int accountId,
        int categoryId,
        int merchantId,
        string? note)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Transactions
(TransactionDate, Amount, TransactionType, AccountId, CategoryId, MerchantId, Note)
VALUES
($date, $amount, $type, $accountId, $categoryId, $merchantId, $note);";

        command.Parameters.AddWithValue("$date", date);
        command.Parameters.AddWithValue("$amount", amount);
        command.Parameters.AddWithValue("$type", type);
        command.Parameters.AddWithValue("$accountId", accountId);
        command.Parameters.AddWithValue("$categoryId", categoryId);
        command.Parameters.AddWithValue("$merchantId", merchantId);
        command.Parameters.AddWithValue("$note", (object?)note ?? DBNull.Value);
        command.ExecuteNonQuery();
    }
}
