using System;
using System.Collections.Generic;

namespace Demo.Entities;

public partial class Stock
{
    public string StockId { get; set; } = null!;

    public string CompanyName { get; set; } = null!;

    public decimal CurrentPrice { get; set; }

    public int Volume { get; set; }

    public DateOnly UpdateDate { get; set; }

    public virtual ICollection<SellBuyRequest> SellBuyRequests { get; set; } = new List<SellBuyRequest>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
