using System; 
namespace OnlineRadioStation.Domain
{
    public class StoppedState : IStreamState
    {
        private readonly DjStream _stream;
        public StoppedState(DjStream stream)
        {
            _stream = stream;
        }
        public void Play()
        {
            Console.WriteLine("Stream is starting...");
            _stream.SetState(new PlayingState(_stream));
        }
        public void Stop()
        {
            Console.WriteLine("Stream is already stopped.");
        }
        public void Pause()
        {
            Console.WriteLine("Cannot pause a stopped stream.");
        }

        public void Resume()
        {
            Console.WriteLine("Cannot resume a stopped stream.");
        }
    }
}