using System;
using System.Collections.Generic;
using FinanceCrudApp.Data;
using FinanceCrudApp.Models;

namespace FinanceCrudApp.Repositories;

public class CategoryRepository
{
    public List<Category> GetAll()
    {
        var categories = new List<Category>();

        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT CategoryId, Name, CategoryType, Description, IsActive
FROM Categories
ORDER BY CategoryId;";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            categories.Add(new Category
            {
                CategoryId = reader.GetInt32(0),
                Name = reader.GetString(1),
                CategoryType = reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                IsActive = reader.GetInt32(4) == 1
            });
        }

        return categories;
    }

    public void Insert(Category category)
    {
        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Categories (Name, CategoryType, Description, IsActive)
VALUES ($name, $type, $description, $isActive);";

        command.Parameters.AddWithValue("$name", category.Name);
        command.Parameters.AddWithValue("$type", category.CategoryType);
        command.Parameters.AddWithValue("$description", (object?)category.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("$isActive", category.IsActive ? 1 : 0);

        command.ExecuteNonQuery();
    }

    public void Update(Category category)
    {
        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE Categories
SET Name = $name,
    CategoryType = $type,
    Description = $description,
    IsActive = $isActive
WHERE CategoryId = $id;";

        command.Parameters.AddWithValue("$name", category.Name);
        command.Parameters.AddWithValue("$type", category.CategoryType);
        command.Parameters.AddWithValue("$description", (object?)category.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("$isActive", category.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("$id", category.CategoryId);

        command.ExecuteNonQuery();
    }

    public void Delete(int categoryId)
    {
        using var connection = DbManager.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
DELETE FROM Categories
WHERE CategoryId = $id;";

        command.Parameters.AddWithValue("$id", categoryId);

        command.ExecuteNonQuery();
    }
}
