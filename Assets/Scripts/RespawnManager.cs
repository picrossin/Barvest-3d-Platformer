using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    private Vector3 _respawnPoint = Vector3.zero;

    public void SetRespawnPoint(Vector3 newRespawnPoint) => _respawnPoint = newRespawnPoint;

    public Vector3 GetRespawnPoint() => _respawnPoint;

    public void Respawn(Transform respawnable) => respawnable.transform.SetPositionAndRotation(_respawnPoint, Quaternion.identity);
}
