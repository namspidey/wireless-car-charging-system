using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class BalanceTransaction
{
    public int TransactionId { get; set; }

    public int BalanceId { get; set; }

    public double? Amount { get; set; }

    public string? TransactionType { get; set; }

    public DateTime? TransactionDate { get; set; }

    public virtual Balance Balance { get; set; } = null!;
}
