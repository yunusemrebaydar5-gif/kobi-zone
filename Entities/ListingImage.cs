using System;
using System.Collections.Generic;

namespace Pazaryeri.Api.Entities;

public partial class ListingImage
{
    public int ImageId { get; set; }

    public int ListingId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool IsMain { get; set; }

    public virtual Listing Listing { get; set; } = null!;
}
