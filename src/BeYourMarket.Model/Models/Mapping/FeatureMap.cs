using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
  public class FeatureMap : EntityTypeConfiguration<Feature>
  {
    public FeatureMap()
    {
      // Primary Key
      this.HasKey(t => t.ID);

      // Properties
      this.Property(t => t.Title)
          .IsRequired()
          .HasMaxLength(500);

      this.Property(t => t.UserID)
          .IsRequired()
          .HasMaxLength(128);

      // Table & Column Mappings
      this.ToTable("Features");
      this.Property(t => t.ID).HasColumnName("ID");
      this.Property(t => t.Title).HasColumnName("Title");
      this.Property(t => t.UserID).HasColumnName("UserID");

      // Relationships
      this.HasRequired(t => t.AspNetUser)
          .WithMany(t => t.Features)
          .HasForeignKey(d => d.UserID).WillCascadeOnDelete();

    }
  }
}
