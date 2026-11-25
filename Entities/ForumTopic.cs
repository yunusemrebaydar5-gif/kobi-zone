using System;
using System.Collections.Generic;

namespace Pazaryeri.Api.Entities;

public partial class ForumTopic
{
    public int TopicId { get; set; }

    public int ForumCategoryId { get; set; }

    public int AuthorUserId { get; set; }

    public string Title { get; set; } = null!;

    public string Body { get; set; } = null!;

    public bool IsClosed { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User AuthorUser { get; set; } = null!;

    public virtual ForumCategory ForumCategory { get; set; } = null!;

    public virtual ICollection<ForumPost> ForumPosts { get; set; } = new List<ForumPost>();
}
