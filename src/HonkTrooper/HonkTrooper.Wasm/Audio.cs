using Microsoft.UI.Xaml;
using System;

namespace HonkTrooper.Wasm
{
    public partial class Audio : FrameworkElement
    {
        #region Fields

        private Action _playback;
        //private string _source;
        //private double _volume;
        //private bool _loop;

        #endregion

        #region Ctor

        public Audio(
            string source,
            double volume = 1.0,
            bool loop = false,
            Action playback = null)
        {
            var audio = "element.style.display = \"none\"; " +
                "element.controls = false; " +
                $"element.src = \"{source}\"; " +
                $"element.volume = {volume}; " +
                $"element.loop = {loop.ToString().ToLower()}; ";

            this.ExecuteJavascript(audio);

            if (playback is not null)
            {
                _playback = playback;
                this.RegisterHtmlEventHandler("ended", EndedEvent);
            }

            Console.WriteLine("source: " + source + " volume: " + volume.ToString() + " loop: " + loop.ToString().ToLower());
        }

        #endregion

        #region Methods

        public void SetSource(string source)
        {
            this.ExecuteJavascript($"element.src = \"{source}\"; ");
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
