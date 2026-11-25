using System;
using System.Collections.Generic;

namespace Pazaryeri.Api.Entities;

public partial class Listing
{
    public int ListingId { get; set; }

    public int CompanyId { get; set; }

    public int CategoryId { get; set; }

    public string ListingType { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? About { get; set; }

    public decimal? Price { get; set; }

    public string? PriceUnit { get; set; }

    public decimal Quantity { get; set; }

    public string? Condition { get; set; }

    public string? SwapWanted { get; set; }

    public DateOnly? AvailableFrom { get; set; }

    public DateOnly? AvailableTo { get; set; }

    public string? City { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual LaborDetail? LaborDetail { get; set; }

    public virtual ICollection<ListingImage> ListingImages { get; set; } = new List<ListingImage>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();
}
