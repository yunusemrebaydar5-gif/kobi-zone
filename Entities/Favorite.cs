using System;
using System.Collections.Generic;

namespace Pazaryeri.Api.Entities;

public partial class Favorite
{
    public int UserId { get; set; }

    public int ListingId { get; set; }

    public DateTime AddedAt { get; set; }

    public virtual Listing Listing { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
