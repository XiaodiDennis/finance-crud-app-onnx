namespace FinanceCrudApp.Models;

public class Account
{
    public int AccountId { get; set; }
    public string Name { get; set; } = "";
    public string AccountType { get; set; } = "";
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }

    public override string ToString()
    {
        string status = IsActive ? "Active" : "Inactive";
        return $"{AccountId} | {Name} | {AccountType} | Balance: {CurrentBalance:F2} | {status}";
    }
}
