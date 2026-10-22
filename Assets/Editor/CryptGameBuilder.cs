using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
[InitializeOnLoad]
public static class CryptGameBuilder
{
    static readonly Color Ink = new Color(.025f,.05f,.07f,.97f);
    static readonly Color Gold = new Color(.82f,.66f,.36f);
    static readonly Color Paper = new Color(.94f,.89f,.77f);
    static TMP_FontAsset font;
    static CryptGameBuilder() { EditorApplication.delayCall += Auto; }
    static void Auto() {
        if(EditorApplication.isPlayingOrWillChangePlaymode || File.Exists("Library/CryptPart2Built.flag")) return;
        if(!AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset")) {
            AssetDatabase.importPackageCompleted += Imported;
            AssetDatabase.ImportPackage("Library/PackageCache/com.unity.textmeshpro@3.0.6/Package Resources/TMP Essential Resources.unitypackage",false); return;
        }
        Upgrade();
    }
    static void Imported(string name) { AssetDatabase.importPackageCompleted -= Imported; EditorApplication.delayCall += Auto; }
    static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position, Vector2 anchor) {
        var o = new GameObject(name,typeof(RectTransform)); var r = o.GetComponent<RectTransform>(); r.SetParent(parent,false);
        r.anchorMin = r.anchorMax = anchor; r.sizeDelta = size; r.anchoredPosition = position; return r;
    }
    static Image Panel(string name, Transform parent, Vector2 size, Vector2 position, Vector2 anchor, Color color, bool blocks=false) {
        var r=Rect(name,parent,size,position,anchor); var i=r.gameObject.AddComponent<Image>(); i.color=color; i.raycastTarget=blocks; return i;
    }
    static void Stretch(RectTransform r) { r.anchorMin=Vector2.zero; r.anchorMax=Vector2.one; r.offsetMin=Vector2.zero; r.offsetMax=Vector2.zero; }
    static TMP_Text Label(string name, Transform parent, string text, int size, Vector2 dimensions, Vector2 position, Vector2 anchor, TextAlignmentOptions align=TextAlignmentOptions.Center) {
        var r=Rect(name,parent,dimensions,position,anchor); var t=r.gameObject.AddComponent<TextMeshProUGUI>(); t.font=font; t.text=text; t.fontSize=size;
        t.color=Paper; t.alignment=align; t.raycastTarget=false; t.enableWordWrapping=true; return t;
    }
    static Button Button(string name,Transform parent,string caption,Vector2 position,UnityAction action,out TMP_Text label) {
        var border=Panel(name,parent,new Vector2(360,56),position,new Vector2(.5f,.5f),Gold,true);
        var button=border.gameObject.AddComponent<Button>(); button.targetGraphic=border;
        var colors=button.colors; colors.normalColor=Color.white; colors.highlightedColor=new Color(1.2f,1.2f,1.1f); colors.pressedColor=new Color(.65f,.65f,.65f); colors.selectedColor=Color.white; button.colors=colors;
        var nav=button.navigation; nav.mode=Navigation.Mode.None; button.navigation=nav;
        Panel("Inset",border.transform,new Vector2(356,52),Vector2.zero,new Vector2(.5f,.5f),new Color(.055f,.105f,.13f));
        label=Label("Label",border.transform,caption,20,new Vector2(344,48),Vector2.zero,new Vector2(.5f,.5f)); label.fontStyle=FontStyles.Bold;
        UnityEventTools.AddPersistentListener(button.onClick,action); return button;
    }
    static Canvas Canvas(Camera camera) {
        var o=new GameObject("Interface",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var c=o.GetComponent<Canvas>(); c.renderMode=RenderMode.ScreenSpaceCamera; c.worldCamera=camera; c.planeDistance=1; c.sortingOrder=3000;
        var scaler=o.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1280,720); scaler.matchWidthOrHeight=.5f;
        new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule)); return c;
    }
    static void FlashPrefab(string path) {
        var root=PrefabUtility.LoadPrefabContents(path); if(!root.GetComponent<DamageFlash>()) root.AddComponent<DamageFlash>();
        PrefabUtility.SaveAsPrefabAsset(root,path); PrefabUtility.UnloadPrefabContents(root);
    }
    [MenuItem("Crypt Survivor/Configure complete game")]
    public static void Upgrade() {
        try {
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            EditorSceneManager.OpenScene("Assets/Scenes/GameArena.unity");
            if(Object.FindObjectOfType<GameplayHUD>()) { File.WriteAllText("Library/CryptPart2Built.flag","OK"); return; }
            FlashPrefab("Assets/Prefabs/Player.prefab"); FlashPrefab("Assets/Prefabs/Skeleton.prefab");
            foreach(var enemy in Object.FindObjectsOfType<EnemyHealth>()) Object.DestroyImmediate(enemy.gameObject);
            var game=Object.FindObjectOfType<GameManager>(); game.Player=Object.FindObjectOfType<PlayerHealth>();
            var waves=game.gameObject.AddComponent<WaveManager>(); var score=game.gameObject.AddComponent<ScoreManager>();
            game.gameObject.AddComponent<GameAudio>(); game.Waves=waves; game.Score=score; score.Waves=waves; waves.Player=game.Player;
            waves.SkeletonPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Skeleton.prefab").GetComponent<EnemyHealth>();
            waves.SpawnPoints=GameObject.Find("Enemy Spawn Points - Part 2").GetComponentsInChildren<Transform>().Skip(1).ToArray();
            if(!Camera.main.GetComponent<ArenaCamera>()) Camera.main.gameObject.AddComponent<ArenaCamera>();
            var canvas=Canvas(Camera.main);
            var header=Panel("HUD backdrop",canvas.transform,Vector2.zero,Vector2.zero,new Vector2(.5f,1),Ink);
            var hr=header.rectTransform; hr.anchorMin=new Vector2(0,1); hr.anchorMax=new Vector2(1,1); hr.pivot=new Vector2(.5f,1); hr.sizeDelta=new Vector2(0,86); hr.anchoredPosition=Vector2.zero;
            var hud=canvas.gameObject.AddComponent<GameplayHUD>(); hud.Game=game;
            hud.HealthLabel=Label("Health",header.transform,"VIDA 100/100",20,new Vector2(260,30),new Vector2(165,-25),new Vector2(0,1),TextAlignmentOptions.Left);
            var health=Panel("HealthBar",header.transform,new Vector2(260,12),new Vector2(165,-54),new Vector2(0,1),new Color(.15f,.2f,.23f));
            var fill=Panel("Fill",health.transform,Vector2.zero,Vector2.zero,new Vector2(.5f,.5f),new Color(.68f,.18f,.18f)); Stretch(fill.rectTransform);
            hud.HealthBar=health.gameObject.AddComponent<Slider>(); hud.HealthBar.fillRect=fill.rectTransform; hud.HealthBar.direction=Slider.Direction.LeftToRight; hud.HealthBar.maxValue=100; hud.HealthBar.value=100; hud.HealthBar.interactable=false;
            hud.WaveLabel=Label("Wave",header.transform,"ONDA 1/3",26,new Vector2(280,50),new Vector2(0,-39),new Vector2(.5f,1)); hud.WaveLabel.color=Gold;
            hud.ScoreLabel=Label("Score",header.transform,"PONTUAÇÃO: 0000",20,new Vector2(320,50),new Vector2(-192,-39),new Vector2(1,1),TextAlignmentOptions.Right);
            Label("Controls",canvas.transform,"WASD / SETAS  •  MOVER       MOUSE  •  MIRAR       BOTÃO ESQUERDO  •  DISPARAR",15,new Vector2(1000,28),new Vector2(0,18),new Vector2(.5f,0));
            hud.AnnouncementLabel=Label("Wave announcement",canvas.transform,"ONDA 1",44,new Vector2(700,90),new Vector2(0,205),new Vector2(.5f,.5f)); hud.AnnouncementLabel.color=Gold; hud.AnnouncementLabel.fontStyle=FontStyles.Bold; hud.AnnouncementLabel.gameObject.SetActive(false);
            var overlay=Panel("Result overlay",canvas.transform,Vector2.zero,Vector2.zero,new Vector2(.5f,.5f),new Color(.005f,.012f,.02f,.87f),true); Stretch(overlay.rectTransform); hud.ResultPanel=overlay.gameObject;
            var card=Panel("Result frame",overlay.transform,new Vector2(720,440),Vector2.zero,new Vector2(.5f,.5f),Gold);
            Panel("Card inset",card.transform,new Vector2(716,436),Vector2.zero,new Vector2(.5f,.5f),Ink);
            hud.ResultTitle=Label("Result title",card.transform,"VITÓRIA!",50,new Vector2(660,72),new Vector2(0,140),new Vector2(.5f,.5f)); hud.ResultTitle.color=Gold; hud.ResultTitle.fontStyle=FontStyles.Bold;
            hud.ResultMessage=Label("Result message",card.transform,"Você sobreviveu às três ondas!",22,new Vector2(660,45),new Vector2(0,78),new Vector2(.5f,.5f));
            hud.FinalScore=Label("Final score",card.transform,"Pontuação Final: 2500",28,new Vector2(660,52),new Vector2(0,23),new Vector2(.5f,.5f));
            Button("Replay",card.transform,"JOGAR NOVAMENTE",new Vector2(0,-58),game.Restart,out hud.ReplayLabel);
            Button("MainMenu",card.transform,"MENU PRINCIPAL",new Vector2(0,-130),game.MainMenu,out _);
            overlay.gameObject.SetActive(false);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            // Reuse the arena environment as the menu background without changing its source.
            var arena=SceneManager.GetActiveScene(); var menu=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            var grid=Object.Instantiate(GameObject.Find("Crypt Tilemaps")); grid.name="Crypt background"; SceneManager.MoveGameObjectToScene(grid,menu);
            var camObject=Object.Instantiate(Camera.main.gameObject); camObject.name="Main Camera"; SceneManager.MoveGameObjectToScene(camObject,menu);
            SceneManager.SetActiveScene(menu); EditorSceneManager.CloseScene(arena,true);
            var menuCanvas=Canvas(camObject.GetComponent<Camera>());
            var shade=Panel("Background shade",menuCanvas.transform,Vector2.zero,Vector2.zero,new Vector2(.5f,.5f),new Color(.01f,.025f,.04f,.8f)); Stretch(shade.rectTransform);
            var controller=new GameObject("MainMenuController").AddComponent<MainMenuController>();
            var top=Label("Eyebrow",menuCanvas.transform,"S O B R E V I V A   À   C R I P T A",16,new Vector2(900,40),new Vector2(0,200),new Vector2(.5f,.5f)); top.color=Gold;
            var title=Label("Title",menuCanvas.transform,"CRYPT\nSURVIVOR",76,new Vector2(1050,190),new Vector2(0,80),new Vector2(.5f,.5f)); title.fontStyle=FontStyles.Bold; title.characterSpacing=12;
            Panel("Gold rule",menuCanvas.transform,new Vector2(120,2),new Vector2(0,-35),new Vector2(.5f,.5f),Gold);
            Label("Description",menuCanvas.transform,"TRÊS ONDAS. VINTE E CINCO ESQUELETOS. UMA CHANCE.",17,new Vector2(1000,40),new Vector2(0,-78),new Vector2(.5f,.5f));
            Button("Play",menuCanvas.transform,"JOGAR",new Vector2(0,-150),controller.Play,out _);
            Button("Quit",menuCanvas.transform,"SAIR",new Vector2(0,-220),controller.Quit,out _);
            Label("Footer",menuCanvas.transform,"WASD / SETAS PARA MOVER   •   MOUSE PARA MIRAR E DISPARAR",15,new Vector2(1100,30),new Vector2(0,38),new Vector2(.5f,0));
            EditorSceneManager.SaveScene(menu,"Assets/Scenes/MainMenu.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity",true),new EditorBuildSettingsScene("Assets/Scenes/GameArena.unity",true)};
            PlayerSettings.productName="Crypt Survivor"; PlayerSettings.defaultScreenWidth=1280; PlayerSettings.defaultScreenHeight=720; PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            AssetDatabase.SaveAssets(); File.WriteAllText("Library/CryptPart2Built.flag","OK"); Debug.Log("CRYPT_PART2_BUILD_OK");
        } catch(System.Exception e) { File.WriteAllText("Logs/CryptPart2SetupError.txt",e.ToString()); Debug.LogException(e); }
    }
}


