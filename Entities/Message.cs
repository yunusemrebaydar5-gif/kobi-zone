using System;
using System.Collections.Generic;

namespace Pazaryeri.Api.Entities;

public partial class Message
{
    public int MessageId { get; set; }

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    public int? ListingId { get; set; }

    public int? QuoteId { get; set; }

    public string Body { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }

    public virtual Listing? Listing { get; set; }

    public virtual Quote? Quote { get; set; }

    public virtual User Receiver { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
