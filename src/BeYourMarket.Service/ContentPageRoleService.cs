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
    public interface IContentPageRoleService : IService<ContentPageRole>
    {
    }

    public class ContentPageRoleService : Service<ContentPageRole>, IContentPageRoleService
    {
        public ContentPageRoleService(IRepositoryAsync<ContentPageRole> repository)
            : base(repository)
        {
        }
    }
}
