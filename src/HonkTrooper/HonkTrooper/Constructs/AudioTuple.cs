namespace HonkTrooper
{
    public partial class AudioTuple
    {
        public AudioWasm[] AudioSources { get; set; } // TODO: audio to implement for mobile native

        public AudioWasm AudioInstance { get; set; } // TODO: audio to implement for mobile native

        public SoundType SoundType { get; set; }

        public AudioTuple(AudioWasm[] audioSources, AudioWasm audioInstance, SoundType soundType)
        {
            AudioSources = audioSources;
            AudioInstance = audioInstance;
            SoundType = soundType;
        }
    }
}
