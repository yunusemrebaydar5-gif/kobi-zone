using System;
using System.Collections.Generic;

namespace Pazaryeri.Api.Entities;

public partial class CompanyReview
{
    public int ReviewId { get; set; }

    public int ContractId { get; set; }

    public int ReviewerCompanyId { get; set; }

    public int ReviewedCompanyId { get; set; }

    public byte Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Contract Contract { get; set; } = null!;

    public virtual Company ReviewedCompany { get; set; } = null!;

    public virtual Company ReviewerCompany { get; set; } = null!;
}
