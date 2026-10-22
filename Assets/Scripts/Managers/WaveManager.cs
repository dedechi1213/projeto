using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[DisallowMultipleComponent]
public class WaveManager : MonoBehaviour
{
    public EnemyHealth SkeletonPrefab;
    public PlayerHealth Player;
    public Transform[] SpawnPoints;
    public float BetweenWaves = 2.5f;
    public float MinimumPlayerDistance = 3f;
    public int CurrentWave { get; private set; }
    public int AliveCount => alive.Count;
    public bool Running { get; private set; }
    public bool Between { get; private set; }
    public event Action<int> WaveChanged;
    public event Action<string> Announcement;
    static readonly int[] Counts = { 5, 8, 12 };
    readonly HashSet<EnemyHealth> alive = new HashSet<EnemyHealth>();
    readonly HashSet<EnemyHealth> spawned = new HashSet<EnemyHealth>();
    void OnEnable() { EnemyHealth.EnemyDied += OnEnemyDied; }
    void OnDisable() { EnemyHealth.EnemyDied -= OnEnemyDied; Stop(); }
    public bool Owns(EnemyHealth enemy) => spawned.Contains(enemy);
    public void Begin() {
        if(Running || CurrentWave != 0 || !GameManager.Instance.IsPlaying) return;
        Running = true; StartCoroutine(RunWaves());
    }
    IEnumerator RunWaves() {
        for(int wave = 1; wave <= 3; wave++) {
            if(!Running || !GameManager.Instance.IsPlaying) yield break;
            Between = false; CurrentWave = wave;
            for(int n = 0; n < Counts[wave-1]; n++) {
                if(!TrySpawnPosition(n, out Vector2 position)) {
                    Debug.LogError("Não foi possível encontrar uma posição válida para o esqueleto."); Stop(); yield break;
                }
                var enemy = Instantiate(SkeletonPrefab, position, Quaternion.identity);
                enemy.GetComponent<EnemyController>().Target = Player;
                spawned.Add(enemy); alive.Add(enemy); Physics2D.SyncTransforms();
            }
            WaveChanged?.Invoke(wave); Announcement?.Invoke(wave == 3 ? "ONDA FINAL" : "ONDA " + wave);
            // Resume on a later frame so every death subscriber has awarded the final score.
            yield return new WaitUntil(() => alive.Count == 0 || !Running);
            if(!Running || !GameManager.Instance.IsPlaying) yield break;
            if(wave == 3) { Running = false; GameManager.Instance.Win(); yield break; }
            Between = true; yield return new WaitForSeconds(BetweenWaves);
        }
    }
    bool Valid(Vector2 point) {
        if(Mathf.Abs(point.x) > 12f || Mathf.Abs(point.y) > 6f || Vector2.Distance(point, Player.transform.position) < MinimumPlayerDistance) return false;
        return Physics2D.OverlapCircle(point, .48f) == null;
    }
    bool TrySpawnPosition(int index, out Vector2 point) {
        if(SpawnPoints != null) for(int ring = 0; ring < 4; ring++) for(int n = 0; n < SpawnPoints.Length; n++) {
            var anchor = SpawnPoints[(index+n)%SpawnPoints.Length]; if(!anchor) continue;
            point = (Vector2)anchor.position + (ring == 0 ? Vector2.zero : new Vector2(Mathf.Cos(index*2.4f+n), Mathf.Sin(index*2.4f+n))*ring);
            if(Valid(point)) return true;
        }
        for(int y = -5; y <= 5; y++) for(int x = -11; x <= 11; x++) { point = new Vector2(x,y); if(Valid(point)) return true; }
        point = Vector2.zero; return false;
    }
    void OnEnemyDied(EnemyHealth enemy) { if(Running) alive.Remove(enemy); }
    public void Stop() { Running = false; Between = false; StopAllCoroutines(); }
}
