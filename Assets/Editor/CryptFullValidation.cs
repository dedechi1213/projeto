using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
[InitializeOnLoad]
public static class CryptFullValidation
{
    static bool testing; static int stage; static double next, deadline; static string result; static int errors;
    static GameManager game => GameManager.Instance;
    static GameplayHUD hud => UnityEngine.Object.FindObjectOfType<GameplayHUD>();
    static EnemyHealth[] Living => UnityEngine.Object.FindObjectsOfType<EnemyHealth>().Where(e=>!e.IsDead).ToArray();
    static CryptFullValidation() { EditorApplication.update += Tick; EditorApplication.playModeStateChanged += Mode; }
    [MenuItem("Crypt Survivor/Run complete game validation")]
    public static void StartTests() { File.WriteAllText("Library/CryptFullCommand.txt","test"); }
    static void Mode(PlayModeStateChange state) {
        if(state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("CryptFullTest",false)) {
            testing=true; stage=0; errors=0; result="Crypt Survivor — validação completa em Play Mode\n";
            next=EditorApplication.timeSinceStartup+.5; deadline=next+90; Application.logMessageReceived+=Log;
        }
        if(state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool("CryptQuitExpected",false)) {
            SessionState.SetBool("CryptQuitExpected",false); SessionState.SetBool("CryptFullTest",false);
            File.AppendAllText("Docs/Validacao-Parte2.txt","PASS: Botão SAIR encerrou o Play Mode no Editor\n");
        }
    }
    static void Log(string message,string trace,LogType type) { if(type == LogType.Exception || type == LogType.Error) { errors++; File.AppendAllText("Logs/CryptFullRuntimeErrors.txt",message+"\n"+trace+"\n"); } }
    static void Check(bool ok,string label) { result+=(ok?"PASS: ":"FAIL: ")+label+"\n"; Directory.CreateDirectory("Docs"); File.WriteAllText("Docs/Validacao-Parte2.txt",result); }
    static void Freeze() { foreach(var e in Living) { e.GetComponent<EnemyController>().enabled=false; e.GetComponent<Rigidbody2D>().velocity=Vector2.zero; } }
    static void KillAll() { foreach(var e in Living) e.TakeDamage(999); }
    static void Click(string name) { var button=UnityEngine.Object.FindObjectsOfType<Button>().Single(b=>b.name==name); Check(button.interactable && button.onClick.GetPersistentEventCount()==1,"Botão conectado: "+name); Canvas.ForceUpdateCanvases();
        var pointer=new PointerEventData(EventSystem.current) { button=PointerEventData.InputButton.Left, position=RectTransformUtility.WorldToScreenPoint(Camera.main,button.transform.position) };
        var hits=new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer,hits);
        Check(hits.Count>0 && hits[0].gameObject==button.gameObject,"Mouse alcança o botão: "+name);
        ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler); }
    static void Tick() {
        if(!EditorApplication.isPlayingOrWillChangePlaymode && File.Exists("Library/CryptFullCommand.txt")) {
            string command=File.ReadAllText("Library/CryptFullCommand.txt").Trim(); File.Delete("Library/CryptFullCommand.txt");
            if(command=="test") { EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity"); SessionState.SetBool("CryptFullTest",true); EditorApplication.isPlaying=true; }
        }
        if(!testing || !EditorApplication.isPlaying || EditorApplication.timeSinceStartup<next) return;
        if(EditorApplication.timeSinceStartup>deadline) { Check(false,"Tempo limite da validação"); Finish(); return; }
        try { Run(); } catch(Exception e) { Check(false,e.ToString()); Finish(); }
    }
    static void Run() {
        float delay=.4f;
        switch(stage) {
        case 0:
            Check(SceneManager.GetActiveScene().name=="MainMenu" && UnityEngine.Object.FindObjectOfType<EventSystem>(),"Menu inicial com EventSystem");
            Check(EditorBuildSettings.scenes.Length==2 && EditorBuildSettings.scenes[0].path.EndsWith("MainMenu.unity"),"MainMenu e GameArena configuradas na build");
            Snapshot("MainMenu"); Snapshot("MainMenu-800x600",800,600); Click("Play"); break;
        case 1:
            Check(SceneManager.GetActiveScene().name=="GameArena" && game.IsPlaying,"JOGAR carrega GameArena");
            Check(game.Player.CurrentHealth==100 && game.Score.Score==0,"Nova partida: vida 100 e pontos zero");
            Check(game.Waves.CurrentWave==1 && game.Waves.AliveCount==5 && Living.Length==5,"Onda 1 gera exatamente cinco esqueletos");
            Check(Living.All(e=>Vector2.Distance(e.transform.position,game.Player.transform.position)>2.5f && Mathf.Abs(e.transform.position.x)<12.5f && Mathf.Abs(e.transform.position.y)<6.5f),"Nascimento longe do jogador e dentro dos limites");
            Freeze(); game.Waves.BetweenWaves=.5f;
            int before=Living.Length; game.Waves.Begin(); Check(Living.Length==before,"Begin repetido não duplica a onda");
            Check(hud.HealthLabel.text=="VIDA 100/100" && hud.ScoreLabel.text.EndsWith("0000") && hud.WaveLabel.text=="ONDA 1/3","HUD inicial conectado");
            var data=new PointerEventData(EventSystem.current) { position=new Vector2(Screen.width/2,Screen.height/2) }; var hits=new List<RaycastResult>(); EventSystem.current.RaycastAll(data,hits);
            Check(hits.Count==0,"HUD deixa clique na arena livre para disparar");
            Check(UnityEngine.Object.FindObjectsOfType<Graphic>().All(g=>g is Image || !g.raycastTarget),"Textos não bloqueiam mouse");
            Snapshot("Gameplay"); Snapshot("Gameplay-800x600",800,600);
            var first=Living[0]; first.TakeDamage(999); first.TakeDamage(999);
            Check(game.Score.Score==100 && game.Waves.AliveCount==4 && hud.ScoreLabel.text.EndsWith("0100"),"Morte concede exatamente 100 pontos uma vez e atualiza HUD");
            game.Player.TakeDamage(15); Check(hud.HealthLabel.text=="VIDA 85/100" && hud.HealthBar.value==85,"Dano atualiza barra e número imediatamente");
            foreach(var e in Living.Take(3)) e.TakeDamage(999); delay=1f; break;
        case 2:
            Check(game.Waves.CurrentWave==1 && game.Waves.AliveCount==1 && game.IsPlaying,"Onda não avança com um inimigo vivo");
            KillAll(); delay=.15f; break;
        case 3:
            Check(game.Waves.CurrentWave==1 && game.Waves.Between && game.Waves.AliveCount==0,"Intervalo entre ondas sem geração simultânea"); delay=.7f; break;
        case 4:
            Check(game.Waves.CurrentWave==2 && game.Waves.AliveCount==8 && Living.Length==8 && game.Score.Score==500,"Onda 2 gera exatamente oito; pontuação 500");
            Check(hud.WaveLabel.text=="ONDA 2/3" && hud.AnnouncementLabel.text=="ONDA 2","Indicador e anúncio da segunda onda");
            Freeze(); KillAll(); delay=.9f; break;
        case 5:
            Check(game.Waves.CurrentWave==3 && game.Waves.AliveCount==12 && Living.Length==12 && game.Score.Score==1300,"Onda 3 gera exatamente doze; pontuação 1300");
            Check(hud.WaveLabel.text=="ONDA 3/3" && hud.AnnouncementLabel.text=="ONDA FINAL","Indicador e anúncio da onda final");
            Freeze(); foreach(var e in Living.Take(11)) e.TakeDamage(999); break;
        case 6:
            Check(game.IsPlaying && game.Waves.AliveCount==1 && game.Score.Score==2400,"Vitória aguarda o último esqueleto"); KillAll(); break;
        case 7:
            Check(game.State==GameState.Victory && !game.Waves.Running && game.Score.Score==2500,"Vitória após 25 mortes e exatamente 2500 pontos");
            Check(hud.ResultPanel.activeSelf && hud.ResultTitle.text=="VITÓRIA!" && hud.FinalScore.text.EndsWith("2500"),"Painel de vitória mostra pontuação final");
            Check(!game.Player.GetComponent<PlayerMovement>().enabled && !game.Player.GetComponent<PlayerAttack>().enabled,"Vitória desativa movimento e disparo");
            game.PlayerDied(); game.Player.TakeDamage(999); Check(game.State==GameState.Victory && game.Player.CurrentHealth==85,"Vitória e derrota mutuamente exclusivas; dano bloqueado");
            Snapshot("Victory"); Snapshot("Victory-800x600",800,600); Click("Replay"); break;
        case 8:
            Check(game.IsPlaying && game.Waves.CurrentWave==1 && game.Waves.AliveCount==5 && game.Score.Score==0 && game.Player.CurrentHealth==100,"Jogar novamente reinicia vida, ondas, inimigos e pontuação");
            Check(UnityEngine.Object.FindObjectsOfType<GameManager>().Length==1 && UnityEngine.Object.FindObjectsOfType<ScoreManager>().Length==1,"Reinício não duplica gerenciadores");
            Freeze(); game.Waves.BetweenWaves=.2f; Living[0].TakeDamage(999);
            Check(game.Score.Score==100,"Reinício não acumula assinaturas de pontuação");
            game.Player.TakeDamage(999); break;
        case 9:
            Check(game.State==GameState.GameOver && !game.Waves.Running && game.Player.IsDead,"Vida zero encerra ondas em Game Over");
            Check(hud.ResultPanel.activeSelf && hud.ResultTitle.text=="GAME OVER" && hud.FinalScore.text.EndsWith("0100") && hud.ReplayLabel.text=="TENTAR NOVAMENTE","Derrota mostra pontos e botão correto");
            Check(!game.Player.GetComponent<PlayerMovement>().enabled && !game.Player.GetComponent<PlayerAttack>().enabled && Living.All(e=>!e.GetComponent<EnemyController>().enabled),"Derrota interrompe todos os controles e inimigos");
            game.Win(); KillAll(); game.Player.GetComponent<PlayerAttack>().Fire(Vector2.right);
            Check(game.State==GameState.GameOver && game.Score.Score==100 && Living.Length==4 && UnityEngine.Object.FindObjectsOfType<Projectile>().Length==0,"Após derrota: sem vitória, novos pontos, dano ou disparos");
            Snapshot("GameOver"); delay=.8f; break;
        case 10:
            Check(game.Waves.CurrentWave==1 && Living.Length==4,"Nenhuma onda gerada após derrota"); Click("Replay"); break;
        case 11:
            Check(game.IsPlaying && game.Player.CurrentHealth==100 && game.Score.Score==0 && Living.Length==5,"Tentar novamente reinicia integralmente");
            Freeze(); game.Player.TakeDamage(999); break;
        case 12:
            Click("MainMenu"); break;
        case 13:
            Check(SceneManager.GetActiveScene().name=="MainMenu" && !GameManager.Instance && Living.Length==0,"Menu principal limpa objetos da partida");
            Check(errors==0,"Nenhum erro ou exceção durante os testes de execução");
            Application.logMessageReceived-=Log; testing=false; SessionState.SetBool("CryptQuitExpected",true); Click("Quit"); break;
        }
        stage++; next=EditorApplication.timeSinceStartup+delay;
    }
    static void Finish() { testing=false; Application.logMessageReceived-=Log; SessionState.SetBool("CryptFullTest",false); EditorApplication.isPlaying=false; }
    public static void Snapshot(string name,int width=1280,int height=720) {
        var camera=Camera.main; if(!camera) return;
        var rt=new RenderTexture(width,height,24); camera.targetTexture=rt; camera.GetComponent<ArenaCamera>()?.Fit(); Canvas.ForceUpdateCanvases(); camera.Render();
        RenderTexture.active=rt; var image=new Texture2D(width,height,TextureFormat.RGB24,false); image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply();
        Directory.CreateDirectory("Docs"); File.WriteAllBytes("Docs/"+name+".png",image.EncodeToPNG()); camera.targetTexture=null; RenderTexture.active=null;
        UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(rt);
    }
}



