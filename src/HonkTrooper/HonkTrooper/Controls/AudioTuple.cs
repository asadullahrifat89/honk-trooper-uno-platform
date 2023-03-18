namespace HonkTrooper
{
    public partial class AudioTuple
    {
        public Audio[] AudioSources { get; set; } // TODO: audio to implement for mobile native

        public Audio AudioInstance { get; set; } // TODO: audio to implement for mobile native

        public SoundType SoundType { get; set; }

        public AudioTuple(Audio[] audioSources, Audio audioInstance, SoundType soundType)
        {
            AudioSources = audioSources;
            AudioInstance = audioInstance;
            SoundType = soundType;
        }
    }
}
