using System;
using System.Collections.Generic;

namespace Pazaryeri.Api.Entities;

public partial class User
{
    public int UserId { get; set; }

    public int? CompanyId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

    public virtual Company? Company { get; set; }

    public virtual ICollection<ContractSignature> ContractSignatures { get; set; } = new List<ContractSignature>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<ForumPost> ForumPosts { get; set; } = new List<ForumPost>();

    public virtual ICollection<ForumTopic> ForumTopics { get; set; } = new List<ForumTopic>();

    public virtual ICollection<Message> MessageReceivers { get; set; } = new List<Message>();

    public virtual ICollection<Message> MessageSenders { get; set; } = new List<Message>();

    public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();
}
