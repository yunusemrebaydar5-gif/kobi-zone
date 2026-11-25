using System;
using System.Collections.Generic;

namespace Pazaryeri.Api.Entities;

public partial class CompanyDocument
{
    public int DocumentId { get; set; }

    public int CompanyId { get; set; }

    public string DocType { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    public virtual Company Company { get; set; } = null!;
}
