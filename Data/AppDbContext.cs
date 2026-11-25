using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Api.Entities;

namespace Pazaryeri.Api.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<CompanyDocument> CompanyDocuments { get; set; }

    public virtual DbSet<CompanyReview> CompanyReviews { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<ContractSignature> ContractSignatures { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<ForumCategory> ForumCategories { get; set; }

    public virtual DbSet<ForumPost> ForumPosts { get; set; }

    public virtual DbSet<ForumTopic> ForumTopics { get; set; }

    public virtual DbSet<LaborDetail> LaborDetails { get; set; }

    public virtual DbSet<Listing> Listings { get; set; }

    public virtual DbSet<ListingImage> ListingImages { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Quote> Quotes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:Default");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0B6DBB970C");

            entity.Property(e => e.CategoryName).HasMaxLength(80);

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK__Categorie__Paren__5FB337D6");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("PK__Companie__2D971CACEAAED31A");

            entity.HasIndex(e => e.TaxNumber, "UQ__Companie__34A7C179BA229F01").IsUnique();

            entity.Property(e => e.About).HasMaxLength(1000);
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.Property(e => e.CompanySize).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.OrganizedZone).HasMaxLength(150);
            entity.Property(e => e.RejectReason).HasMaxLength(300);
            entity.Property(e => e.Sector).HasMaxLength(100);
            entity.Property(e => e.TaxNumber)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.TaxOffice).HasMaxLength(100);
            entity.Property(e => e.TradeRegistryNo).HasMaxLength(30);
            entity.Property(e => e.VerificationStatus)
                .HasMaxLength(10)
                .HasDefaultValue("pending");

            entity.HasOne(d => d.VerifiedByUser).WithMany(p => p.Companies)
                .HasForeignKey(d => d.VerifiedByUserId)
                .HasConstraintName("FK__Companies__Verif__5629CD9C");
        });

        modelBuilder.Entity<CompanyDocument>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__CompanyD__1ABEEF0FE911178B");

            entity.Property(e => e.DocType).HasMaxLength(30);
            entity.Property(e => e.FileUrl).HasMaxLength(500);
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Company).WithMany(p => p.CompanyDocuments)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK__CompanyDo__Compa__5AEE82B9");
        });

        modelBuilder.Entity<CompanyReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__CompanyR__74BC79CEF611312B");

            entity.HasIndex(e => new { e.ContractId, e.ReviewerCompanyId }, "UQ_Review").IsUnique();

            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Contract).WithMany(p => p.CompanyReviews)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CompanyRe__Contr__160F4887");

            entity.HasOne(d => d.ReviewedCompany).WithMany(p => p.CompanyReviewReviewedCompanies)
                .HasForeignKey(d => d.ReviewedCompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CompanyRe__Revie__17F790F9");

            entity.HasOne(d => d.ReviewerCompany).WithMany(p => p.CompanyReviewReviewerCompanies)
                .HasForeignKey(d => d.ReviewerCompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CompanyRe__Revie__17036CC0");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PK__Contract__C90D3469C367B00E");

            entity.HasIndex(e => e.QuoteId, "UQ__Contract__AF9688C0BEF71DA9").IsUnique();

            entity.HasIndex(e => e.ContractNo, "UQ__Contract__C908F4B8B0B12ACF").IsUnique();

            entity.Property(e => e.ContentHash)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ContractNo).HasMaxLength(30);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("awaiting_signature");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(14, 2)");

            entity.HasOne(d => d.Quote).WithOne(p => p.Contract)
                .HasForeignKey<Contract>(d => d.QuoteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Contracts__Quote__09A971A2");
        });

        modelBuilder.Entity<ContractSignature>(entity =>
        {
            entity.HasKey(e => new { e.ContractId, e.CompanyId }).HasName("PK__Contract__1BD445A37651E93E");

            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.SignatureHash)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.SignedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Company).WithMany(p => p.ContractSignatures)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ContractS__Compa__10566F31");

            entity.HasOne(d => d.Contract).WithMany(p => p.ContractSignatures)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK__ContractS__Contr__0F624AF8");

            entity.HasOne(d => d.SignedByUser).WithMany(p => p.ContractSignatures)
                .HasForeignKey(d => d.SignedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ContractS__Signe__114A936A");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ListingId }).HasName("PK__Favorite__0C7B27A1B6D9A805");

            entity.Property(e => e.AddedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Listing).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Favorites__Listi__778AC167");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Favorites__UserI__76969D2E");
        });

        modelBuilder.Entity<ForumCategory>(entity =>
        {
            entity.HasKey(e => e.ForumCategoryId).HasName("PK__ForumCat__FACAC18D38020D53");

            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.Name).HasMaxLength(80);
        });

        modelBuilder.Entity<ForumPost>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__ForumPos__AA126018D1340E79");

            entity.Property(e => e.Body).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.AuthorUser).WithMany(p => p.ForumPosts)
                .HasForeignKey(d => d.AuthorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ForumPost__Autho__2CF2ADDF");

            entity.HasOne(d => d.Topic).WithMany(p => p.ForumPosts)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK__ForumPost__Topic__2BFE89A6");
        });

        modelBuilder.Entity<ForumTopic>(entity =>
        {
            entity.HasKey(e => e.TopicId).HasName("PK__ForumTop__022E0F5DA2E48249");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.AuthorUser).WithMany(p => p.ForumTopics)
                .HasForeignKey(d => d.AuthorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ForumTopi__Autho__2739D489");

            entity.HasOne(d => d.ForumCategory).WithMany(p => p.ForumTopics)
                .HasForeignKey(d => d.ForumCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ForumTopi__Forum__2645B050");
        });

        modelBuilder.Entity<LaborDetail>(entity =>
        {
            entity.HasKey(e => e.ListingId).HasName("PK__LaborDet__BF3EBED018DD37E0");

            entity.Property(e => e.ListingId).ValueGeneratedNever();
            entity.Property(e => e.Certifications).HasMaxLength(300);
            entity.Property(e => e.HeadCount).HasDefaultValue(1);
            entity.Property(e => e.SkillTitle).HasMaxLength(100);

            entity.HasOne(d => d.Listing).WithOne(p => p.LaborDetail)
                .HasForeignKey<LaborDetail>(d => d.ListingId)
                .HasConstraintName("FK__LaborDeta__Listi__71D1E811");
        });

        modelBuilder.Entity<Listing>(entity =>
        {
            entity.HasKey(e => e.ListingId).HasName("PK__Listings__BF3EBED0469F907C");

            entity.HasIndex(e => e.CompanyId, "IX_Listings_Company");

            entity.HasIndex(e => new { e.ListingType, e.CategoryId }, "IX_Listings_Type_Cat").HasFilter("([IsActive]=(1))");

            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.Condition).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ListingType).HasMaxLength(20);
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.PriceUnit).HasMaxLength(10);
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(12, 2)");
            entity.Property(e => e.SwapWanted).HasMaxLength(300);
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.Category).WithMany(p => p.Listings)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Listings__Catego__6383C8BA");

            entity.HasOne(d => d.Company).WithMany(p => p.Listings)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Listings__Compan__628FA481");
        });

        modelBuilder.Entity<ListingImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__ListingI__7516F70C4553BE8A");

            entity.Property(e => e.ImageUrl).HasMaxLength(300);

            entity.HasOne(d => d.Listing).WithMany(p => p.ListingImages)
                .HasForeignKey(d => d.ListingId)
                .HasConstraintName("FK__ListingIm__Listi__6E01572D");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Messages__C87C0C9C56E8E0FB");

            entity.Property(e => e.Body).HasMaxLength(1000);
            entity.Property(e => e.SentAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Listing).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ListingId)
                .HasConstraintName("FK__Messages__Listin__1EA48E88");

            entity.HasOne(d => d.Quote).WithMany(p => p.Messages)
                .HasForeignKey(d => d.QuoteId)
                .HasConstraintName("FK__Messages__QuoteI__1F98B2C1");

            entity.HasOne(d => d.Receiver).WithMany(p => p.MessageReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Messages__Receiv__1DB06A4F");

            entity.HasOne(d => d.Sender).WithMany(p => p.MessageSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Messages__Sender__1CBC4616");
        });

        modelBuilder.Entity<Quote>(entity =>
        {
            entity.HasKey(e => e.QuoteId).HasName("PK__Quotes__AF9688C132F94E43");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.ProposedPrice).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(12)
                .HasDefaultValue("pending");
            entity.Property(e => e.SwapOffer).HasMaxLength(300);

            entity.HasOne(d => d.BuyerCompany).WithMany(p => p.QuoteBuyerCompanies)
                .HasForeignKey(d => d.BuyerCompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Quotes__BuyerCom__7C4F7684");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Quotes)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Quotes__CreatedB__7E37BEF6");

            entity.HasOne(d => d.Listing).WithMany(p => p.Quotes)
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Quotes__ListingI__7B5B524B");

            entity.HasOne(d => d.ParentQuote).WithMany(p => p.InverseParentQuote)
                .HasForeignKey(d => d.ParentQuoteId)
                .HasConstraintName("FK__Quotes__ParentQu__7F2BE32F");

            entity.HasOne(d => d.SellerCompany).WithMany(p => p.QuoteSellerCompanies)
                .HasForeignKey(d => d.SellerCompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Quotes__SellerCo__7D439ABD");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C10D0CF74");

            entity.HasIndex(e => e.Phone, "UQ__Users__5C7E359ECA38B3BC").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105349C59B791").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(250);
            entity.Property(e => e.Phone)
                .HasMaxLength(11)
                .IsFixedLength();
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("company_user");

            entity.HasOne(d => d.Company).WithMany(p => p.Users)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK_Users_Company");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
