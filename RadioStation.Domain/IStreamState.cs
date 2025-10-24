using System.Threading.Tasks; 

namespace OnlineRadioStation.Domain
{
    public interface IStreamState
    {
        void Play();

        void Stop();

        void Pause();

        void Resume();
    }
}