using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
  public class ListingFeatureMap : EntityTypeConfiguration<ListingFeature>
  {
    public ListingFeatureMap()
    {
      // Primary Key
      this.HasKey(t => t.ID);

      // Table & Column Mappings
      this.ToTable("ListingFeature");
      this.Property(t => t.ID).HasColumnName("ID");
      this.Property(t => t.FeatureID).HasColumnName("FeatureID");
      this.Property(t => t.ListingID).HasColumnName("ListingID");

      // Relationships
      this.HasRequired(t => t.Feature)
       .WithMany(t => t.ListingFeatures)
       .HasForeignKey(d => d.FeatureID).WillCascadeOnDelete();
      this.HasRequired(t => t.Listing)
       .WithMany(t => t.ListingFeatures)
       .HasForeignKey(d => d.ListingID).WillCascadeOnDelete();
    }
  }
}
