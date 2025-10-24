using System;
namespace OnlineRadioStation.Domain
{
    public class PausedState : IStreamState
    {
        private readonly DjStream _stream;
        public PausedState(DjStream stream)
        {
            _stream = stream;
        }

        public void Play()
        {
            Console.WriteLine("Cannot play a paused stream. Use Resume.");
        }
        public void Stop()
        {
            Console.WriteLine("Stream is stopping from pause...");
            _stream.SetState(new StoppedState(_stream));
        }
        public void Pause()
        {
            Console.WriteLine("Stream is already paused.");
        }
        public void Resume()
        {
            Console.WriteLine("Stream is resuming...");
            _stream.SetState(new PlayingState(_stream));
        }
    }
}