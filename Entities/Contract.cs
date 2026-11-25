using System;
using System.Collections.Generic;

namespace Pazaryeri.Api.Entities;

public partial class Contract
{
    public int ContractId { get; set; }

    public int QuoteId { get; set; }

    public string ContractNo { get; set; } = null!;

    public string TermsText { get; set; } = null!;

    public decimal? TotalAmount { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string ContentHash { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CompanyReview> CompanyReviews { get; set; } = new List<CompanyReview>();

    public virtual ICollection<ContractSignature> ContractSignatures { get; set; } = new List<ContractSignature>();

    public virtual Quote Quote { get; set; } = null!;
}
