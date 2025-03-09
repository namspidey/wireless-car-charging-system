using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Balance
{
    public int BalanceId { get; set; }

    public int UserId { get; set; }

    public double? Balance1 { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual ICollection<BalanceTransaction> BalanceTransactions { get; set; } = new List<BalanceTransaction>();

    public virtual User User { get; set; } = null!;
}
