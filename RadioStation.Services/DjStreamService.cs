using OnlineRadioStation.Data;
using OnlineRadioStation.Domain;
using System;
using System.Threading.Tasks;

namespace OnlineRadioStation.Services
{
    public class DjStreamService : IDjStreamService
    {
        private readonly IDjStreamRepository _streamRepository;

        public DjStreamService(IDjStreamRepository streamRepository)
        {
            _streamRepository = streamRepository;
        }

        private async Task<DjStream> FindStreamOrFailAsync(Guid streamId)
        {
            var stream = await _streamRepository.GetById(streamId);
            if (stream == null)
            {
                throw new Exception($"Стрім з ID {streamId} не знайдено.");
            }
            // Потрібно буде відновити стан при завантаженні (наприклад, за полем Status у БД)
            // Поки що ми працюємо з об'єктом у пам'яті.
            return stream;
        }

        public async Task<DjStream?> GetOrCreateCurrentStreamAsync(Guid djId, Guid stationId)
        {
             var existingStream = await _streamRepository.GetActiveStreamByDjIdAsync(djId);
             if (existingStream != null)
             {
                 return existingStream;
             }

             // Повноцінна реалізація має зберегти його в БД
             var newStream = new DjStream
             {
                 StreamId = Guid.NewGuid(), 
                 DjId = djId,
                 StationId = stationId, 
                 StartTime = DateTime.UtcNow 
             };
              // _streamRepository.AddEntity(newStream); // Потрібно буде зберегти
              // await _streamRepository.SaveChangesAsync();
             return newStream;
        }


        public async Task StartStreamAsync(Guid streamId)
        {
            var stream = await FindStreamOrFailAsync(streamId);
            stream.StartStream(); 
            // TODO: Зберегти зміни стану в БД (наприклад, оновити поле Status)
            // await _streamRepository.SaveChangesAsync();
        }

        public async Task StopStreamAsync(Guid streamId)
        {
            var stream = await FindStreamOrFailAsync(streamId);
            stream.StopStream();
            stream.EndTime = DateTime.UtcNow; 
            _streamRepository.UpdateEntity(stream); 
            await _streamRepository.SaveChangesAsync();
        }

        public async Task PauseStreamAsync(Guid streamId)
        {
            var stream = await FindStreamOrFailAsync(streamId);
            stream.PauseStream();
            // TODO: Зберегти зміни стану в БД
            // await _streamRepository.SaveChangesAsync();
        }

        public async Task ResumeStreamAsync(Guid streamId)
        {
            var stream = await FindStreamOrFailAsync(streamId);
            stream.ResumeStream();
            // TODO: Зберегти зміни стану в БД
            // await _streamRepository.SaveChangesAsync();
        }
    }
}