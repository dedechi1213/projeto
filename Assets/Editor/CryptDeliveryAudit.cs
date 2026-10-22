using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
[InitializeOnLoad]
public static class CryptDeliveryAudit
{
    static CryptDeliveryAudit() { EditorApplication.update+=Tick; }
    static void Tick() {
        if(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists("Library/CryptAuditCommand.txt")) return;
        File.Delete("Library/CryptAuditCommand.txt"); Run();
    }
    [MenuItem("Crypt Survivor/Audit scene and prefab references")]
    public static void Run() {
        string result="Auditoria de cenas e prefabs\n"; int missing=0;
        foreach(string name in new[]{"GameArena","MainMenu"}) {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");
            if(!Camera.main.GetComponent<ArenaCamera>()) { Camera.main.gameObject.AddComponent<ArenaCamera>(); EditorSceneManager.SaveScene(scene); }
            if(name=="GameArena") {
                var game=UnityEngine.Object.FindObjectOfType<GameManager>();
                if(!game.GetComponent<GameAudio>()) { game.gameObject.AddComponent<GameAudio>(); EditorSceneManager.SaveScene(scene); }
                bool linked=game.Player && game.Waves && game.Score && game.Waves.Player && game.Waves.SkeletonPrefab && game.Score.Waves && UnityEngine.Object.FindObjectOfType<GameplayHUD>().Game;
                result+=(linked?"PASS: ":"FAIL: ")+"Referências dos gerenciadores e HUD\n";
            }
            foreach(var root in scene.GetRootGameObjects()) foreach(var t in root.GetComponentsInChildren<Transform>(true)) missing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
            result+="PASS: Cena carregada: "+name+"\n";
        }
        foreach(string file in Directory.GetFiles("Assets/Prefabs","*.prefab")) {
            var root=PrefabUtility.LoadPrefabContents(file.Replace('\\','/'));
            foreach(var t in root.GetComponentsInChildren<Transform>(true)) missing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
            if(root.GetComponent<Animator>()) result+=(root.GetComponent<Animator>().runtimeAnimatorController?"PASS: ":"FAIL: ")+"Animator do prefab "+root.name+"\n";
            if(root.GetComponent<PlayerAttack>()) result+=(root.GetComponent<PlayerAttack>().ProjectilePrefab?"PASS: ":"FAIL: ")+"Referência do projétil no jogador\n";
            PrefabUtility.UnloadPrefabContents(root);
        }
        result+=(missing==0?"PASS: ":"FAIL: ")+"Missing Scripts: "+missing+"\n";
        Directory.CreateDirectory("Docs"); File.WriteAllText("Docs/Auditoria-Final.txt",result); Debug.Log(result);
    }
}


