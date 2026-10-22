using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GameplayHUD : MonoBehaviour
{
    public GameManager Game;
    public TMP_Text HealthLabel, ScoreLabel, WaveLabel, AnnouncementLabel;
    public Slider HealthBar;
    public GameObject ResultPanel;
    public TMP_Text ResultTitle, ResultMessage, FinalScore, ReplayLabel;
    Coroutine banner;
    void OnEnable() {
        Game.Player.HealthChanged += Health;
        Game.Score.ScoreChanged += Score;
        Game.Waves.WaveChanged += Wave;
        Game.Waves.Announcement += Announce;
        Game.StateChanged += Result;
    }
    void Start() { Health(Game.Player.CurrentHealth, Game.Player.MaxHealth); Score(Game.Score.Score); Wave(Game.Waves.CurrentWave); ResultPanel.SetActive(false); }
    void OnDisable() {
        if(!Game) return;
        Game.Player.HealthChanged -= Health; Game.Score.ScoreChanged -= Score;
        Game.Waves.WaveChanged -= Wave; Game.Waves.Announcement -= Announce; Game.StateChanged -= Result;
    }
    void Health(int current, int maximum) { HealthLabel.text = "VIDA " + current + "/" + maximum; HealthBar.maxValue = maximum; HealthBar.value = current; }
    void Score(int value) { ScoreLabel.text = "PONTUAÇÃO: " + value.ToString("D4"); }
    void Wave(int value) { WaveLabel.text = "ONDA " + Mathf.Max(1,value) + "/3"; }
    void Announce(string message) { if(banner != null) StopCoroutine(banner); banner = StartCoroutine(Banner(message)); }
    IEnumerator Banner(string message) { AnnouncementLabel.text = message; AnnouncementLabel.gameObject.SetActive(true); yield return new WaitForSeconds(1.8f); AnnouncementLabel.gameObject.SetActive(false); }
    void Result(GameState state) {
        if(banner != null) StopCoroutine(banner); AnnouncementLabel.gameObject.SetActive(false);
        ResultTitle.text = state == GameState.Victory ? "VITÓRIA!" : "GAME OVER";
        ResultMessage.text = state == GameState.Victory ? "Você sobreviveu às três ondas!" : "Você não conseguiu sobreviver.";
        FinalScore.text = "Pontuação Final: " + Game.Score.Score.ToString("D4");
        ReplayLabel.text = state == GameState.Victory ? "JOGAR NOVAMENTE" : "TENTAR NOVAMENTE";
        ResultPanel.SetActive(true);
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }
}
