using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class Withdraw_Request
{
    public long withdraw_id { get; set; }

    public long user_id { get; set; }

    public decimal amount { get; set; }

    public string bank_account { get; set; } = null!;

    public string status { get; set; } = null!;

    public DateTime requested_at { get; set; }

    public DateTime? processed_at { get; set; }

    public virtual User user { get; set; } = null!;
}
