namespace FinanceCrudApp.Models;

public class Merchant
{
    public int MerchantId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public override string ToString()
    {
        string status = IsActive ? "Active" : "Inactive";
        return $"{MerchantId} | {Name} | {status}";
    }
}
