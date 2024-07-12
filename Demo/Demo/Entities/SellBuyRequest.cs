using System;
using System.Collections.Generic;

namespace Demo.Entities;

public partial class SellBuyRequest
{
    public int RequestId { get; set; }

    public string? UserId { get; set; }

    public string? StockId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public DateTime RequestDate { get; set; }

    public string RequestType { get; set; } = null!;

    public virtual Stock? Stock { get; set; }

    public virtual User? User { get; set; }
}
