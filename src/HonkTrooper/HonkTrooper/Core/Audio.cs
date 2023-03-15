using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using Uno.UI.Runtime.WebAssembly;

namespace HonkTrooper
{
    [HtmlElement("audio")]
    public partial class Audio : FrameworkElement
    {
        #region Fields

        private Action _playback;
        private string _baseUrl;

        #endregion

        #region Properties

        public bool IsPlaying { get; set; }

        public bool IsPaused { get; set; }

        public bool IsStopped { get; set; }

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
                playback: playback);
        }

        #endregion

        #region Methods

        public void Initialize(
            Uri uri,
            double volume = 1.0,
            bool loop = false,
            Action playback = null)
        {
            var indexUrl = Uno.Foundation.WebAssemblyRuntime.InvokeJS("window.location.href;");
            var appPackageId = Environment.GetEnvironmentVariable("UNO_BOOTSTRAP_APP_BASE");

            _baseUrl = $"{indexUrl}{appPackageId}";

            var audio = "element.style.display = \"none\"; " +
                "element.controls = false;";

            this.ExecuteJavascript(audio);

            SetSource(uri);
            SetVolume(volume);
            SetLoop(loop);

            if (playback is not null)
            {
                _playback = playback;
                this.RegisterHtmlEventHandler("ended", EndedEvent);
            }

            //Console.WriteLine("source: " + uri + " volume: " + volume.ToString() + " loop: " + loop.ToString().ToLower());
        }

        public void SetSource(Uri uri)
        {
            var source = $"{_baseUrl}/{uri.AbsoluteUri.Replace("ms-appx:///", "")}";

            this.ExecuteJavascript($"element.src = \"{source}\"; ");

            Console.WriteLine("source: " + source);
        }

        public void SetLoop(bool loop)
        {
            this.ExecuteJavascript($"element.loop = {loop.ToString().ToLower()};");
        }

        public void Play()
        {
            IsPlaying = true;
            IsStopped = false;
            this.ExecuteJavascript("element.currentTime = 0; element.play();");
        }

        public void Stop()
        {
            IsPlaying = false;
            IsStopped = true;
            this.ExecuteJavascript("element.pause(); element.currentTime = 0;");
        }

        public void Pause()
        {
            IsPlaying = false;
            IsPaused = true;
            this.ExecuteJavascript("element.pause();");
        }

        public void Resume()
        {
            IsPlaying = true;
            IsPaused = true;
            this.ExecuteJavascript("element.play();");
        }

        public void SetVolume(double volume)
        {
            var audio = $"element.volume = {volume}; ";
            this.ExecuteJavascript(audio);
        }

        #endregion

        #region Events

        private void EndedEvent(object sender, EventArgs e)
        {
            _playback?.Invoke();
        }

        #endregion
    }
}
