using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using WMPLib;

namespace MP3
{
    public class WindowsMediaPlayerProvider
    {
        private WindowsMediaPlayer _player;

        private PlayState _playState;

        public event Action OnPlayEnd;
        public event Action OnPlayStart;

        public PlayState GetPlayState()
        {
            return _playState;
        }

        public bool IsPlaying()
        {
            return _playState == PlayState.Playing;
        }

        public bool IsPaused()
        {
            return _playState == PlayState.Paused;
        }

        public float GetNormalizedPosition()
        {
            if(_playState != PlayState.NonInitialized)
            {
                if (Math.Abs(GetDurationSeconds()) < 0.00000001f)
                {
                    return 0f;
                }
                return GetElapsedSeconds() / GetDurationSeconds();
            }
            return 0f;
        }

        public float GetElapsedSeconds()
        {
            if(_playState != PlayState.NonInitialized)
            {
                return (float)_player.controls.currentPosition;
            }
            return 0f;
        }

        public float GetDurationSeconds()
        {
            if(_playState != PlayState.NonInitialized)
            {
                return (float)_player.currentMedia.duration;
            }
            return 0f;
        }

        public void SetNormalizedPosition(float position)
        {
            if(_playState != PlayState.NonInitialized)
            {
                _player.controls.currentPosition = position * _player.currentMedia.duration;
            }
        }
    }
    
}
