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
  public interface IFeatureService : IService<Feature>
  {
  }

  public class FeatureService : Service<Feature>, IFeatureService
  {
    public FeatureService(IRepositoryAsync<Feature> repository)
        : base(repository)
    {
    }
  }
}
