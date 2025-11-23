using System;
using System.Threading.Tasks;

namespace OnlineRadioStation.Domain
{
    public interface IAudioProcessor
    {
        Task ProcessNewTrackAsync(string tempFilePath, string title, Guid stationId, Guid djId);
        Task UploadToLibraryAsync(string tempFilePath, string title, Guid adminId);
    }
}
