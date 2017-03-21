using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
  public partial class ListingFeature : Repository.Pattern.Ef6.Entity
  {
    public int ID { get; set; }
    public virtual int FeatureID { get; set; }
    public virtual Feature Feature { get; set; }
    public virtual int ListingID { get; set; }
    public virtual Listing Listing { get; set; }
  }
}
