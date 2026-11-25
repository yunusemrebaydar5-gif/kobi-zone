using System;
using System.Collections.Generic;

namespace Pazaryeri.Api.Entities;

public partial class LaborDetail
{
    public int ListingId { get; set; }

    public string SkillTitle { get; set; } = null!;

    public int HeadCount { get; set; }

    public int? ExperienceYears { get; set; }

    public string? Certifications { get; set; }

    public virtual Listing Listing { get; set; } = null!;
}
