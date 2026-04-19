namespace FinanceCrudApp.Models;

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = "";
    public string CategoryType { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public override string ToString()
    {
        string status = IsActive ? "Active" : "Inactive";
        return $"{CategoryId} | {Name} | {CategoryType} | {status}";
    }
}
