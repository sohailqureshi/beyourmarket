using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
  public partial class ContentPageRole : Repository.Pattern.Ef6.Entity
  {
    public int ID { get; set; }
    public int ContentPageID { get; set; }
    public virtual ContentPage ContentPage { get; set; }
    public string AspNetRoleID { get; set; }
    public virtual AspNetRole AspNetRole { get; set; }
  }
}
