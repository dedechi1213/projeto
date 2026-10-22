using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum GameState { Playing, Victory, GameOver }
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState State { get; private set; } = GameState.Playing;
    public bool IsPlayerDead => State == GameState.GameOver;
    public bool IsPlaying => State == GameState.Playing;
    public PlayerHealth Player;
    public WaveManager Waves;
    public ScoreManager Score;
    public event Action PlayerDeath;
    public event Action<GameState> StateChanged;
    void Awake() { Instance = this; Time.timeScale = 1f; }
    void Start() { Score.ResetScore(); Waves.Begin(); }
    void OnDestroy() { if(Instance == this) Instance = null; }
    public void PlayerDied() { if(!IsPlaying) return; End(GameState.GameOver); PlayerDeath?.Invoke(); }
    public void Win() { if(IsPlaying) End(GameState.Victory); }
    void End(GameState state) {
        State = state; Waves.Stop();
        Player.GetComponent<PlayerMovement>().enabled = false;
        Player.GetComponent<PlayerAttack>().enabled = false;
        Player.GetComponent<PlayerAnimationController>().enabled = false;
        Player.GetComponent<Animator>().enabled = false;
        foreach(var enemy in FindObjectsOfType<EnemyController>()) { enemy.enabled = false; enemy.GetComponent<Rigidbody2D>().simulated = false; }
        foreach(var shot in FindObjectsOfType<Projectile>()) Destroy(shot.gameObject);
        GameAudio.Play(state == GameState.Victory ? GameCue.Victory : GameCue.Defeat); StateChanged?.Invoke(state);
    }
    public void Restart() { Time.timeScale = 1; SceneManager.LoadScene("GameArena"); }
    public void MainMenu() { Time.timeScale = 1; SceneManager.LoadScene("MainMenu"); }
}

