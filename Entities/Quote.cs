using System;
using System.Collections.Generic;

namespace Pazaryeri.Api.Entities;

public partial class Quote
{
    public int QuoteId { get; set; }

    public int ListingId { get; set; }

    public int BuyerCompanyId { get; set; }

    public int SellerCompanyId { get; set; }

    public int CreatedByUserId { get; set; }

    public int? ParentQuoteId { get; set; }

    public decimal? ProposedPrice { get; set; }

    public decimal Quantity { get; set; }

    public string? SwapOffer { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Note { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Company BuyerCompany { get; set; } = null!;

    public virtual Contract? Contract { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual ICollection<Quote> InverseParentQuote { get; set; } = new List<Quote>();

    public virtual Listing Listing { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Quote? ParentQuote { get; set; }

    public virtual Company SellerCompany { get; set; } = null!;
}
