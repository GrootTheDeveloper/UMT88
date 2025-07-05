using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class PasswordReset




{
    public long id { get; set; }

    public string email { get; set; } = null!;

    public string code { get; set; } = null!;

    public DateTime expires_at { get; set; }

    public bool used { get; set; }
}
