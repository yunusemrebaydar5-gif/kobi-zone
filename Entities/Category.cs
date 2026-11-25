using System;
using System.Collections.Generic;

namespace Pazaryeri.Api.Entities;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public int? ParentCategoryId { get; set; }

    public virtual ICollection<Category> InverseParentCategory { get; set; } = new List<Category>();

    public virtual ICollection<Listing> Listings { get; set; } = new List<Listing>();

    public virtual Category? ParentCategory { get; set; }
}
