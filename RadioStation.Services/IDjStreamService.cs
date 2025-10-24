using OnlineRadioStation.Domain;
using System;
using System.Threading.Tasks;

namespace OnlineRadioStation.Services
{
    public interface IDjStreamService
    {
        Task<DjStream?> GetOrCreateCurrentStreamAsync(Guid djId, Guid stationId); 
        Task StartStreamAsync(Guid streamId);
        Task StopStreamAsync(Guid streamId);
        Task PauseStreamAsync(Guid streamId);
        Task ResumeStreamAsync(Guid streamId);
        // TODO: Додати метод для отримання поточного стану стріму
    }
}