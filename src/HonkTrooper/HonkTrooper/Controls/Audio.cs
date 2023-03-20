using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Media.Core;

#if !__ANDROID__ && !__IOS__
using Uno.UI.Runtime.WebAssembly;
#endif

namespace HonkTrooper
{

#if __ANDROID__ || __IOS__

    public partial class Audio : AudioBase
    {
        private readonly MediaPlayerElement _player;

        public Audio(
            Uri uri,
            double volume = 1.0,
            bool loop = false,
            Action playback = null)
        {
            _player = new MediaPlayerElement() { AreTransportControlsEnabled = false, };

            Windows.Media.Playback.MediaPlayer mediaPlayer = new();

            _player.SetMediaPlayer(mediaPlayer);

            Initialize(
                uri: uri,
                volume: volume,
                loop: loop,
                trackEnded: playback);
        }

        #region Methods

        public void Initialize(
            Uri uri,
            double volume = 1.0,
            bool loop = false,
            Action trackEnded = null)
        {
            try
            {
                SetSource(uri);
                SetVolume(volume);
                SetLoop(loop);

                if (trackEnded is not null)
                {
                    TrackEnded = trackEnded;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message.ToString());
            }
        }

        public void SetSource(Uri uri)
        {
            if (_player.MediaPlayer is not null)
            {
                Uri newUri = new(uri.OriginalString.Replace("ms-appx:///HonkTrooper/Assets/Sounds/", "ms-appx:///Assets/Sounds/"));
                _player.MediaPlayer.Source = MediaSource.CreateFromUri(newUri);
            }
        }

        public void SetLoop(bool loop)
        {
            if (_player.MediaPlayer is not null)
            {
                _player.MediaPlayer.IsLoopingEnabled = loop;
            }
        }

        public new void Play()
        {
            if (_player.MediaPlayer is not null)
            {
                _player.MediaPlayer.Play();
                base.Play();
            }
        }

        public new void Stop()
        {
            if (_player.MediaPlayer is not null)
            {
                _player.MediaPlayer.Stop();
                base.Stop();
            }
        }

        public new void Pause()
        {
            if (_player.MediaPlayer is not null)
            {
                _player.MediaPlayer.Pause();
                base.Pause();
            }
        }

        public new void Resume()
        {
            if (_player.MediaPlayer is not null)
            {
                _player.MediaPlayer.Play();
                base.Resume();
            }
        }

        public new void SetVolume(double volume)
        {
            if (_player.MediaPlayer is not null)
            {
                _player.MediaPlayer.Volume = volume * 100;
                base.SetVolume(volume);
            }
        }

        #endregion
    }

#else


    [HtmlElement("audio")]
    public partial class Audio : AudioBase
    {
        #region Fields

        private string _baseUrl;

        #endregion

        #region Ctor

        public Audio(
           Uri uri,
           double volume = 1.0,
           bool loop = false,
           Action playback = null)
        {
            Initialize(
                uri: uri,
                volume: volume,
                loop: loop,
                trackEnded: playback);
        }

        #endregion

        #region Methods

        public void Initialize(
            Uri uri,
            double volume = 1.0,
            bool loop = false,
            Action trackEnded = null)
        {
            var indexUrl = Uno.Foundation.WebAssemblyRuntime.InvokeJS("window.location.href;");
            var appPackageId = Environment.GetEnvironmentVariable("UNO_BOOTSTRAP_APP_BASE");

            _baseUrl = $"{indexUrl}{appPackageId}";

            var audio = "element.style.display = \"none\"; element.controls = false; element.hidden= \"hidden\"";

            this.ExecuteJavascript(audio);

            SetSource(uri);
            SetVolume(volume);
            SetLoop(loop);

            if (trackEnded is not null)
            {
                TrackEnded = trackEnded;
                this.RegisterHtmlEventHandler("ended", EndedEvent);
            }

            LoggerExtensions.Log("source: " + uri + " volume: " + volume.ToString() + " loop: " + loop.ToString().ToLower());
        }

        public void SetSource(Uri uri)
        {
            var source = $"{_baseUrl}/{uri.AbsoluteUri.Replace("ms-appx:///", "")}";

            this.ExecuteJavascript($"element.src = \"{source}\"; ");

            LoggerExtensions.Log("source: " + source);
        }

        public void SetLoop(bool loop)
        {
            this.ExecuteJavascript($"element.loop = {loop.ToString().ToLower()};");
        }

        public new void Play()
        {
            base.Play();
            this.ExecuteJavascript("element.currentTime = 0; element.play();");
        }

        public new void Stop()
        {
            base.Stop();
            this.ExecuteJavascript("element.pause(); element.currentTime = 0;");
        }

        public new void Pause()
        {
            base.Pause();
            this.ExecuteJavascript("element.pause();");
        }

        public new void Resume()
        {
            base.Resume();
            this.ExecuteJavascript("element.play();");
        }

        public new void SetVolume(double volume)
        {
            base.SetVolume(volume);
            this.ExecuteJavascript($"element.volume = {volume};");
        }

        #endregion
    }

#endif
}
