using System;
using System.Collections.Generic;

namespace Demo.Entities;

public partial class User
{
    public string UserId { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Phone { get; set; }

    public decimal AccountBalance { get; set; }

    public string? Role { get; set; }

    public virtual ICollection<Refreshtoken> Refreshtokens { get; set; } = new List<Refreshtoken>();

    public virtual ICollection<SellBuyRequest> SellBuyRequests { get; set; } = new List<SellBuyRequest>();

    public virtual ICollection<Transaction> TransactionReceivers { get; set; } = new List<Transaction>();

    public virtual ICollection<Transaction> TransactionSenders { get; set; } = new List<Transaction>();
}
