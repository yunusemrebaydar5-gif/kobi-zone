using System;
using System.Collections.Generic;

namespace Pazaryeri.Api.Entities;

public partial class Company
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string TaxNumber { get; set; } = null!;

    public string TaxOffice { get; set; } = null!;

    public string? TradeRegistryNo { get; set; }

    public string? OrganizedZone { get; set; }

    public string? CompanySize { get; set; }

    public string? Sector { get; set; }

    public string City { get; set; } = null!;

    public string? Address { get; set; }

    public string? About { get; set; }

    public string? LogoUrl { get; set; }

    public string VerificationStatus { get; set; } = null!;

    public DateTime? VerifiedAt { get; set; }

    public int? VerifiedByUserId { get; set; }

    public string? RejectReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CompanyDocument> CompanyDocuments { get; set; } = new List<CompanyDocument>();

    public virtual ICollection<CompanyReview> CompanyReviewReviewedCompanies { get; set; } = new List<CompanyReview>();

    public virtual ICollection<CompanyReview> CompanyReviewReviewerCompanies { get; set; } = new List<CompanyReview>();

    public virtual ICollection<ContractSignature> ContractSignatures { get; set; } = new List<ContractSignature>();

    public virtual ICollection<Listing> Listings { get; set; } = new List<Listing>();

    public virtual ICollection<Quote> QuoteBuyerCompanies { get; set; } = new List<Quote>();

    public virtual ICollection<Quote> QuoteSellerCompanies { get; set; } = new List<Quote>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public virtual User? VerifiedByUser { get; set; }
}
