using BeYourMarket.Model.Models;
using Repository.Pattern.Repositories;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Service
{
  public interface IListingFeatureService : IService<ListingFeature>
  {
  }

  public class ListingFeatureService : Service<ListingFeature>, IListingFeatureService
  {
    public ListingFeatureService(IRepositoryAsync<ListingFeature> repository)
        : base(repository)
    {
    }
  }
}
