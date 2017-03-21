using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
  public partial class Feature : Repository.Pattern.Ef6.Entity
  {
    public Feature()
    {
      this.ListingFeatures = new List<ListingFeature>();
    }

    public int ID { get; set; }
    public string Title { get; set; }
    public string UserID { get; set; }
    public virtual AspNetUser AspNetUser { get; set; }
    public virtual ICollection<ListingFeature> ListingFeatures { get; set; }
  }
}
