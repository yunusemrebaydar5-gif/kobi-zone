using System;
using System.Collections.Generic;

namespace Pazaryeri.Api.Entities;

public partial class ForumCategory
{
    public int ForumCategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<ForumTopic> ForumTopics { get; set; } = new List<ForumTopic>();
}
