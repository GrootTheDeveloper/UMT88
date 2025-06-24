using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class Transaction
{
    public long transaction_id { get; set; }

    public long user_id { get; set; }

    public string transaction_type { get; set; } = null!;

    public decimal amount { get; set; }

    public decimal balance_after { get; set; }

    public DateTime created_at { get; set; }

    public virtual User user { get; set; } = null!;
}
