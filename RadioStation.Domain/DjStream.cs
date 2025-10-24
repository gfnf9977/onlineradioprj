using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineRadioStation.Domain
{
    public class DjStream
    {
        private IStreamState _currentState; 
        [Key]
        public Guid StreamId { get; set; }
        public Guid StationId { get; set; }
        public Guid DjId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public RadioStationEntity Station { get; set; } = null!;
        public User Dj { get; set; } = null!;

        public DjStream()
        {
            _currentState = new StoppedState(this); 
        }

        public void SetState(IStreamState newState)
        {
            _currentState = newState;
            Console.WriteLine($"Stream state changed to: {newState.GetType().Name}"); 
        }

        public void StartStream()
        {
            _currentState.Play();
        }

        public void StopStream()
        {
            _currentState.Stop();
        }

        public void PauseStream()
        {
            _currentState.Pause();
        }

        public void ResumeStream()
        {
            _currentState.Resume();
        }
    }
}