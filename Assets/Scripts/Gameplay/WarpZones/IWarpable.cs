using UnityEngine;

public interface IWarpable
{
    public bool IsWarping { get; }
    public Transform TeleportedFrom { get; }
    public void TeleportTo(Transform tpFrom, Vector3 tpTo, Vector3 forward);
    public void DoneTeleporting();
}