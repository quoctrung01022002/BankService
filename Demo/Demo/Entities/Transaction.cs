using System;
using System.Collections.Generic;

namespace Demo.Entities;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public string SenderId { get; set; } = null!;

    public string? ReceiverId { get; set; }

    public string StockId { get; set; } = null!;

    public int Quantity { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal CurrentPrice { get; set; }

    public DateTime TransactionDate { get; set; }

    public virtual User? Receiver { get; set; }

    public virtual User Sender { get; set; } = null!;

    public virtual Stock Stock { get; set; } = null!;
}
