namespace HonkTrooper
{
    public partial class AudioTuple
    {
        public AudioWasm[] AudioSources { get; set; }

        public AudioWasm AudioInstance { get; set; }

        public SoundType SoundType { get; set; }

        public AudioTuple(AudioWasm[] audioSources, AudioWasm audioInstance, SoundType soundType)
        {
            AudioSources = audioSources;
            AudioInstance = audioInstance;
            SoundType = soundType;
        }
    }
}
