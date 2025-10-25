using System;
using System.Threading.Tasks;

namespace OnlineRadioStation.Services 
{
    public class AddFavoriteStationCommand : ICommand
    {
        private readonly IFavoriteService _favoriteService;
        private readonly Guid _userId;
        private readonly Guid _stationId;

        public AddFavoriteStationCommand(IFavoriteService favoriteService, Guid userId, Guid stationId)
        {
            _favoriteService = favoriteService;
            _userId = userId;
            _stationId = stationId;
        }

        public async Task ExecuteAsync()
        {
            await _favoriteService.AddFavoriteAsync(_userId, _stationId);
        }
    }
}