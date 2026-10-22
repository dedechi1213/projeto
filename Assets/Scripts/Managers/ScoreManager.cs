using System;
using System.Collections.Generic;
using UnityEngine;
[DisallowMultipleComponent]
public class ScoreManager : MonoBehaviour
{
    public WaveManager Waves;
    public int Score { get; private set; }
    public event Action<int> ScoreChanged;
    readonly HashSet<int> counted = new HashSet<int>();
    void OnEnable() { EnemyHealth.EnemyDied += OnEnemyDied; }
    void OnDisable() { EnemyHealth.EnemyDied -= OnEnemyDied; }
    public void ResetScore() { Score = 0; counted.Clear(); ScoreChanged?.Invoke(Score); }
    void OnEnemyDied(EnemyHealth enemy) {
        if(!GameManager.Instance || !GameManager.Instance.IsPlaying || !Waves.Owns(enemy) || !counted.Add(enemy.GetInstanceID())) return;
        Score += 100; ScoreChanged?.Invoke(Score);
    }
}
