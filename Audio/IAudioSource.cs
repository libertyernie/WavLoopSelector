namespace WavLoopSelector.Audio
{
    public interface IAudioSource
    {
        IAudioStream[] CreateStreams();
    }
}
