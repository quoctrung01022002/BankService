using System;
using System.Collections.Generic;

namespace Demo.Entities;

public partial class Refreshtoken
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public string? Token { get; set; }

    public string? JwtId { get; set; }

    public bool IsAccount { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime? IssueAt { get; set; }

    public DateTime? ExpireAt { get; set; }

    public virtual User? User { get; set; }
}
