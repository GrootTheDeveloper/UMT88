using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class Deposit_Request
{
    public long deposit_id { get; set; }

    public long user_id { get; set; }

    public decimal amount { get; set; }

    public string payment_method { get; set; } = null!;

    public string status { get; set; } = null!;

    public DateTime requested_at { get; set; }

    public DateTime? processed_at { get; set; }

    public virtual User user { get; set; } = null!;
}
