using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class User
{
    public long user_id { get; set; }

    public string name { get; set; } = null!;

    public string email { get; set; } = null!;

    public string password_hash { get; set; } = null!;

    public decimal balance { get; set; }

    public string status { get; set; } = null!;

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<Bet> Bets { get; set; } = new List<Bet>();

    public virtual ICollection<Deposit_Request> Deposit_Requests { get; set; } = new List<Deposit_Request>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<User_Promotion> User_Promotions { get; set; } = new List<User_Promotion>();

    public virtual ICollection<Withdraw_Request> Withdraw_Requests { get; set; } = new List<Withdraw_Request>();
}
