using BeYourMarket.Model.Models;
using Repository.Pattern.Repositories;
using Service.Pattern;

namespace BeYourMarket.Service
{
  public interface IMetaFieldService : IService<MetaField>
  {
  }

  public class MetaFieldService : Service<MetaField>, IMetaFieldService
  {
    public MetaFieldService(IRepositoryAsync<MetaField> repository)
        : base(repository)
    {
    }
  }
}
