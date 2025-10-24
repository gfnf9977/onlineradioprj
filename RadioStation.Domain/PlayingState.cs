using System;
namespace OnlineRadioStation.Domain
{
    public class PlayingState : IStreamState
    {
        private readonly DjStream _stream;

        public PlayingState(DjStream stream)
        {
            _stream = stream;
        }
        public void Play()
        {
            Console.WriteLine("Stream is already playing.");
        }
        public void Stop()
        {
            Console.WriteLine("Stream is stopping...");
            _stream.SetState(new StoppedState(_stream));
        }
        public void Pause()
        {
            Console.WriteLine("Stream is pausing...");
            _stream.SetState(new PausedState(_stream));
        }
        public void Resume()
        {
            Console.WriteLine("Stream is already playing, cannot resume.");
        }
    }
}