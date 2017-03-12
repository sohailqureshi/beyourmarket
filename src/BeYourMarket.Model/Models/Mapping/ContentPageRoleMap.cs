using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
  public class ContentPageRoleMap : EntityTypeConfiguration<ContentPageRole>
  {
    public ContentPageRoleMap()
    {
      // Primary Key
      this.HasKey(t => t.ID);

      // Table & Column Mappings
      this.ToTable("ContentPageRoles");
      this.Property(t => t.ID).HasColumnName("ID");
      this.Property(t => t.ContentPageID).HasColumnName("ContentPageID");
      this.Property(t => t.AspNetRoleID).HasColumnName("AspNetRoleID");

      // Relationships
      this.HasRequired(t => t.ContentPage)
          .WithMany(t => t.ContentPageRoles)
          .HasForeignKey(d => d.ContentPageID).WillCascadeOnDelete();
      this.HasRequired(t => t.AspNetRole)
          .WithMany(t => t.ContentPageRoles)
          .HasForeignKey(d => d.AspNetRoleID).WillCascadeOnDelete();
    }
  }
}
