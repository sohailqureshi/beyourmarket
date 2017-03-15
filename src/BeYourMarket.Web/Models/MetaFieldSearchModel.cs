using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Models
{
  public partial class MetaFieldSearchModel
  {
    public int ID { get; set; }
    public string Name { get; set; }
    public string Placeholder { get; set; }
    public int ControlTypeID { get; set; }
    public string Options { get; set; }
    public string Selected { get; set; }
    public Nullable<int> Ordering { get; set; }

  }
}