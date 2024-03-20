namespace Service.Timer
{
    public interface ITimerService
    {
        public ITimerHandle CreateTimer(double duration, System.Action callback = null, bool isLooping = false);
        public void DisposeTimer(ITimerHandle timer);
    }
}