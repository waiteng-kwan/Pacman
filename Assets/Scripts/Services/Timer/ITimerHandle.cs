namespace Service.Timer
{
    public interface ITimerHandle
    {
        public void Play(double duration, System.Action callback = null, bool isLooping = false);
        public void Pause();
        public void Stop();
        public void Reset();
        public bool IsPlaying();
        public bool IsFinished();
        public bool Enabled { get; }
    }
}