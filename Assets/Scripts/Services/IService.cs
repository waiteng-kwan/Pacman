namespace Service
{
    public interface IService
    {
        public void Initialize();
        public void Shutdown();
        public void RegisterServiceType(ServiceType serviceType, IService svc);
    }
}