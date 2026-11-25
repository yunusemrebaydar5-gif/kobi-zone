using System;
using System.Collections.Generic;

namespace Pazaryeri.Api.Entities;

public partial class ForumPost
{
    public int PostId { get; set; }

    public int TopicId { get; set; }

    public int AuthorUserId { get; set; }

    public string Body { get; set; } = null!;

    public bool IsSolution { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User AuthorUser { get; set; } = null!;

    public virtual ForumTopic Topic { get; set; } = null!;
}
