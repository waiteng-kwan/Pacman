namespace Service.CoroutineSvc
{
    public interface ICoroutineHandle
    {
        public ICoroutineHandle CreateCoroutine(System.Action callback);
        public void DisposeCoroutine();
    }
}