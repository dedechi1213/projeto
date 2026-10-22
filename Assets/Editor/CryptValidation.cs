using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class CryptValidation
{
    static int stage; static float started; static PlayerHealth player; static EnemyHealth enemy; static Vector3 initial; static int deathEvents;
    static string results; static bool testing;
    static CryptValidation() { EditorApplication.update += Tick; EditorApplication.playModeStateChanged += Mode; }
    [MenuItem("Crypt Survivor/Run combat validation")]
    public static void ValidateMenu() { File.WriteAllText("Library/CryptCommand.txt","test"); }
    [MenuItem("Crypt Survivor/Render arena preview")]
    public static void PreviewMenu() { File.WriteAllText("Library/CryptCommand.txt","preview"); }
    static void Mode(PlayModeStateChange state) {
        if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("CryptTest",false)) {
            testing=true; stage=0; deathEvents=0; results="Crypt Survivor — verificações em Play Mode\n"; started=Time.time;
        }
        if(state==PlayModeStateChange.EnteredEditMode) SessionState.SetBool("CryptTest",false);
    }
    static void Check(bool ok,string label) { results+=(ok?"PASS: ":"FAIL: ")+label+"\n"; File.WriteAllText("Logs/CryptValidation.txt",results); }
    static void Tick() {
        if(!EditorApplication.isPlayingOrWillChangePlaymode && File.Exists("Library/CryptCommand.txt")) {
            string command=File.ReadAllText("Library/CryptCommand.txt").Trim(); File.Delete("Library/CryptCommand.txt");
            if(command=="test") { EditorSceneManager.OpenScene("Assets/Scenes/GameArena.unity"); SessionState.SetBool("CryptTest",true); EditorApplication.isPlaying=true; }
            if(command=="preview") Preview();
            if(command=="build") {
                var report=UnityEditor.Build.Reporting.BuildResult.Unknown;
                try { Directory.CreateDirectory("Build"); report=BuildPipeline.BuildPlayer(new[]{"Assets/Scenes/MainMenu.unity","Assets/Scenes/GameArena.unity"},"Build/Crypt Survivor.exe",BuildTarget.StandaloneWindows64,BuildOptions.None).summary.result; }
                catch(Exception e) { Debug.LogException(e); }
                File.WriteAllText("Logs/CryptBuild.txt",report.ToString());
            }
        }
        if(!testing || !EditorApplication.isPlaying || Time.time-started < .25f) return;
        try { Run(); } catch(Exception e) { Check(false,e.ToString()); Finish(); }
    }
    static void Run() {
        switch(stage) {
        case 0:
            player=UnityEngine.Object.FindObjectOfType<PlayerHealth>();
            Check(player && player.CurrentHealth==100,"Jogador inicia com 100 HP");
            Check(UnityEngine.Object.FindObjectsOfType<EnemyHealth>().Length==5,"Cinco esqueletos de teste");
            Check(UnityEngine.Object.FindObjectsOfType<Tilemap>().Length==2,"Piso e paredes em Tilemaps");
            foreach(var e in UnityEngine.Object.FindObjectsOfType<EnemyHealth>()) { e.transform.position=new Vector3(10,5,0); e.GetComponent<EnemyController>().enabled=false; }
            enemy=UnityEngine.Object.FindObjectOfType<EnemyHealth>(); enemy.transform.position=new Vector3(4,0,0); enemy.GetComponent<EnemyController>().enabled=true; initial=enemy.transform.position;
            break;
        case 1:
            Check(enemy.transform.position.x < initial.x,"Esqueleto persegue jogador com física");
            enemy.GetComponent<EnemyController>().enabled=false; enemy.transform.position=new Vector3(2,0,0);
            player.GetComponent<PlayerAttack>().Fire(Vector2.right); break;
        case 2:
            Check(enemy.CurrentHealth==25,"Projétil acerta e causa 25 de dano");
            Check(UnityEngine.Object.FindObjectsOfType<Projectile>().Length==0,"Projétil é removido no impacto");
            Check(player.CurrentHealth==100,"Disparo não atinge o próprio jogador");
            EnemyHealth.EnemyDied+=CountDeath; player.GetComponent<PlayerAttack>().Fire(Vector2.right); break;
        case 3:
            Check(enemy.IsDead && !enemy.GetComponent<Collider2D>().enabled && !enemy.GetComponent<EnemyController>().enabled,"Morte desativa colisão e ataque do esqueleto");
            Check(enemy.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Death"),"Animação de morte é reproduzida");
            Check(deathEvents==1,"Evento de morte emitido uma vez");
            enemy.TakeDamage(99); Check(deathEvents==1,"Inimigo morto ignora dano adicional");
            player.GetComponent<PlayerMovement>().enabled=false; player.transform.position=new Vector3(12,0,0); player.GetComponent<Rigidbody2D>().velocity=Vector2.right*6; break;
        case 4:
            Check(player.transform.position.x <=12.7f,"Colisor da parede impede saída da arena");
            player.GetComponent<Rigidbody2D>().velocity=Vector2.zero; player.transform.position=Vector3.zero;
            Check(enemy==null,"Esqueleto removido após animação");
            enemy=UnityEngine.Object.FindObjectOfType<EnemyHealth>(); enemy.transform.position=new Vector3(.5f,0,0); enemy.GetComponent<EnemyController>().enabled=true; break;
        case 5:
            Check(player.CurrentHealth==90,"Contato aplica dano");
            initial=new Vector3(player.CurrentHealth,0,0); break;
        case 6:
            Check(player.CurrentHealth==initial.x,"Intervalo impede dano a cada frame");
            enemy.GetComponent<EnemyController>().enabled=false; enemy.transform.position=new Vector3(9,5,0);
            player.GetComponent<PlayerAttack>().ProjectileLifetime=.3f; player.GetComponent<PlayerAttack>().Fire(Vector2.left); break;
        case 7:
            Check(UnityEngine.Object.FindObjectsOfType<Projectile>().Length==0,"Projétil expira sem impacto");
            int updates=0; int deaths=0; player.HealthChanged+=(a,b)=>updates++; player.Died+=()=>deaths++;
            player.TakeDamage(999); player.TakeDamage(10);
            Check(player.IsDead && player.CurrentHealth==0 && updates==1 && deaths==1,"Vida limitada a zero e eventos únicos");
            Check(!player.GetComponent<PlayerMovement>().enabled && !player.GetComponent<PlayerAttack>().enabled && GameManager.Instance.IsPlayerDead,"Morte desativa controles e informa GameManager");
            Finish(); return;
        }
        stage++; started=Time.time;
        // Stages 4 and 5 require a longer physics interval; stage 6 checks the cooldown.
        if(stage==4) started+=1.3f;
        if(stage==5) started+=.1f; if(stage==7) started+=.2f;
    }
    static void CountDeath(EnemyHealth e) { deathEvents++; }
    static void Finish() { EnemyHealth.EnemyDied-=CountDeath; testing=false; SessionState.SetBool("CryptTest",false); EditorApplication.isPlaying=false; Debug.Log("CRYPT_VALIDATION_FINISHED\n"+results); }
    static void Preview() {
        var camera=Camera.main; var rt=new RenderTexture(1280,720,24); camera.targetTexture=rt; camera.Render();
        RenderTexture.active=rt; var image=new Texture2D(1280,720,TextureFormat.RGB24,false); image.ReadPixels(new Rect(0,0,1280,720),0,0); image.Apply();
        Directory.CreateDirectory("Docs"); File.WriteAllBytes("Docs/GameArena.png",image.EncodeToPNG()); camera.targetTexture=null; RenderTexture.active=null;
        UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(rt);
    }
}



