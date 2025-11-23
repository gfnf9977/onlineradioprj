using OnlineRadioStation.Domain;
using System;
namespace OnlineRadioStation.Data
{
    public class LikeDislikeRepository : RepositoryBase<LikeDislike, Guid>, ILikeDislikeRepository
    {
        public LikeDislikeRepository(ApplicationContext context) : base(context) { }
    }
}