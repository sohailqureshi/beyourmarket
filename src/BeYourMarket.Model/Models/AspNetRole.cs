using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
  public partial class AspNetRole : Repository.Pattern.Ef6.Entity
  {
    public AspNetRole()
    {
      this.AspNetUsers = new List<AspNetUser>();
      this.ContentPageRoles = new List<ContentPageRole>();
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public virtual ICollection<AspNetUser> AspNetUsers { get; set; }
    public virtual ICollection<ContentPageRole> ContentPageRoles { get; set; }

  }
}
