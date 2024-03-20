using Client.Services.Vibration;

public interface IVibrationService
{
    public RumblePattern RumblePattern { get; }
    public void StartRumble(RumblePattern pattern);
    public void StartRumble(VibrationDataBase data);
    public void StopRumble();
    public void StopAllRumble();
    public bool IsRumblingNow();
    public void SetDuration(float duration);
}