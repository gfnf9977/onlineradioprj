using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineRadioStation.Domain
{
    public class PlaybackQueueIterator : IPlaylistIterator
    {
        private readonly List<Track> _tracks;
        private int _currentIndex = 0;

        public PlaybackQueueIterator(List<Track> tracks)
        {
            _tracks = tracks;
        }

        public void First() => _currentIndex = 0;

        public void Next() => _currentIndex++;

        public bool IsDone() => _currentIndex >= _tracks.Count;

        public Track Current()
        {
            if (IsDone())
                throw new InvalidOperationException("Кінець плейлиста");
            return _tracks[_currentIndex];
        }
    }
}