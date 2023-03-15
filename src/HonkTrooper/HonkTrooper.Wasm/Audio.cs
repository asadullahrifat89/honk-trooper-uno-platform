using Microsoft.UI.Xaml;
using System;

namespace HonkTrooper.Wasm
{
    public partial class Audio : FrameworkElement
    {
        #region Fields

        private Action _playback;
        private string _baseUrl;

        #endregion

        #region Ctor

        public Audio(
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

            Console.WriteLine("source: " + uri + " volume: " + volume.ToString() + " loop: " + loop.ToString().ToLower());
        }

        #endregion

        #region Methods

        public void SetSource(Uri uri)
        {
            var source = $"{_baseUrl}/{uri.AbsoluteUri.Replace("ms-appx:///", "")}";

            this.ExecuteJavascript($"element.src = \"{source}\"; ");
        }

        public void SetLoop(bool loop)
        {
            this.ExecuteJavascript($"element.loop = {loop.ToString().ToLower()};");
        }

        public void Play()
        {
            this.ExecuteJavascript("element.currentTime = 0; element.play();");
        }

        public void Stop()
        {
            this.ExecuteJavascript("element.pause(); element.currentTime = 0;");
        }

        public void Pause()
        {
            this.ExecuteJavascript("element.pause();");
        }

        public void Resume()
        {
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
