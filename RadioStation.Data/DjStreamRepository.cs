using OnlineRadioStation.Domain;
using System;

namespace OnlineRadioStation.Data
{
    public class DjStreamRepository : RepositoryBase<DjStream, Guid>, IDjStreamRepository
    {
        public DjStreamRepository(ApplicationContext context) : base(context)
        {
        }
    }
}