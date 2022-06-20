using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [Header("Spawn monster")]
    public bool isSpawnMonster;
    private void Awake() => PlayerSpawnSystem.AddSpawnPoint(this.transform, isSpawnMonster);
    private void OnDestroy() => PlayerSpawnSystem.RemoveSpawnPoint(this.transform, isSpawnMonster);
}
