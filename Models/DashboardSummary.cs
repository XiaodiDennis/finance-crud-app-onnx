namespace FinanceCrudApp.Models;

public class DashboardSummary
{
    public int CategoryCount { get; set; }
    public int MerchantCount { get; set; }
    public int AccountCount { get; set; }
    public int TransactionCount { get; set; }

    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetBalance { get; set; }
}
