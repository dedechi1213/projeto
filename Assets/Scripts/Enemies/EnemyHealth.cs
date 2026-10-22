using System;
using UnityEngine;
[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    public int MaxHealth = 50;
    public float DeathDuration = 1.3f;
    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }
    public static event Action<EnemyHealth> EnemyDied;
    public event Action Died;
    void Awake() { CurrentHealth = MaxHealth; }
    public void TakeDamage(int amount) {
        if(IsDead || amount <= 0 || (GameManager.Instance && !GameManager.Instance.IsPlaying)) return;
        GetComponent<DamageFlash>()?.Flash(); CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        if(CurrentHealth > 0) return;
        IsDead = true; GameAudio.Play(GameCue.EnemyDeath); GetComponent<EnemyController>().enabled = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero; GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<Collider2D>().enabled = false; GetComponent<Animator>().Play("Death", 0, 0);
        Died?.Invoke(); EnemyDied?.Invoke(this); Destroy(gameObject, DeathDuration);
    }
}



