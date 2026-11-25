using System;
using System.Collections.Generic;

namespace Pazaryeri.Api.Entities;

public partial class ContractSignature
{
    public int ContractId { get; set; }

    public int CompanyId { get; set; }

    public int SignedByUserId { get; set; }

    public DateTime SignedAt { get; set; }

    public string? IpAddress { get; set; }

    public string SignatureHash { get; set; } = null!;

    public virtual Company Company { get; set; } = null!;

    public virtual Contract Contract { get; set; } = null!;

    public virtual User SignedByUser { get; set; } = null!;
}
