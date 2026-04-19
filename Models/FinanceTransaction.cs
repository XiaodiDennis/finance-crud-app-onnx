using System;

namespace FinanceCrudApp.Models;

public class FinanceTransaction
{
    public int TransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = "";

    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public int MerchantId { get; set; }

    public string? Note { get; set; }

    public string AccountName { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string MerchantName { get; set; } = "";

    public override string ToString()
    {
        return $"{TransactionId} | {TransactionDate:yyyy-MM-dd} | {Amount:F2} | {TransactionType} | {AccountName} | {CategoryName} | {MerchantName}";
    }
}
