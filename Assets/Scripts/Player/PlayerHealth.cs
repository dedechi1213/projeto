using System;
using UnityEngine;
public class PlayerHealth : MonoBehaviour
{
    public int MaxHealth = 100;
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;
    public event Action<int, int> HealthChanged;
    public event Action Died;
    void Awake() { CurrentHealth = MaxHealth; }
    public void TakeDamage(int amount) {
        if(IsDead || amount <= 0 || (GameManager.Instance && !GameManager.Instance.IsPlaying)) return;
        GetComponent<DamageFlash>()?.Flash(); GameAudio.Play(GameCue.Damage); CurrentHealth = Mathf.Max(0, CurrentHealth - amount); HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        if(!IsDead) return;
        GetComponent<PlayerMovement>().enabled = false; GetComponent<PlayerAttack>().enabled = false;
        GetComponent<PlayerAnimationController>().enabled = false; GetComponent<Animator>().enabled = false;
        Died?.Invoke(); if(GameManager.Instance) GameManager.Instance.PlayerDied();
    }
}


