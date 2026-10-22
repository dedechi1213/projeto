using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class CryptArenaBuilder
{
    const string ScenePath = "Assets/Scenes/GameArena.unity";
    static CryptArenaBuilder() { EditorApplication.delayCall += AutoBuild; }
    static void AutoBuild() {
        if(EditorApplication.isPlayingOrWillChangePlaymode || File.Exists("Library/CryptArenaBuilt.flag")) return;
        try { Build(); File.WriteAllText("Library/CryptArenaBuilt.flag", "OK"); }
        catch(Exception e) { Debug.LogException(e); File.WriteAllText("Library/CryptArenaBuildError.txt", e.ToString()); }
    }
    static Sprite[] Slice(string path, int w, int h, float ppu) {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = ppu; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 4096;
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        var sprites = new System.Collections.Generic.List<SpriteMetaData>();
        for(int y = 0; y < texture.height / h; y++) for(int x = 0; x < texture.width / w; x++)
            sprites.Add(new SpriteMetaData { name = "f" + sprites.Count.ToString("D3"), rect = new Rect(x*w, texture.height-(y+1)*h, w,h), pivot = new Vector2(.5f,.5f), alignment = 9 });
        importer.spritesheet = sprites.ToArray(); importer.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>int.Parse(s.name.Substring(1))).ToArray();
    }
    static Sprite Single(string path) {
        var i=(TextureImporter)AssetImporter.GetAtPath(path); i.textureType=TextureImporterType.Sprite; i.spriteImportMode=SpriteImportMode.Single;
        i.spritePixelsPerUnit=32; i.filterMode=FilterMode.Point; i.mipmapEnabled=false; i.textureCompression=TextureImporterCompression.Uncompressed; i.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    static AnimationClip Clip(string name, Sprite[] frames, float fps, bool loop) {
        var clip = new AnimationClip { frameRate = fps };
        var keys = frames.Select((s,n)=>new ObjectReferenceKeyframe { time=n/fps, value=s }).ToList();
        keys.Add(new ObjectReferenceKeyframe { time=frames.Length/fps, value=frames[frames.Length-1] });
        AnimationUtility.SetObjectReferenceCurve(clip,new EditorCurveBinding { path="", type=typeof(SpriteRenderer), propertyName="m_Sprite" },keys.ToArray());
        var settings=AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime=loop; AnimationUtility.SetAnimationClipSettings(clip,settings);
        AssetDatabase.CreateAsset(clip,"Assets/Animations/"+name+".anim"); return clip;
    }
    static void State(AnimatorController c,string name, AnimationClip clip) { var s=c.layers[0].stateMachine.AddState(name); s.motion=clip; }
    static Rigidbody2D Body(GameObject o) { var b=o.AddComponent<Rigidbody2D>(); b.gravityScale=0; b.freezeRotation=true; b.interpolation=RigidbodyInterpolation2D.Interpolate; b.collisionDetectionMode=CollisionDetectionMode2D.Continuous; return b; }
    static GameObject Actor(string name,Sprite sprite,RuntimeAnimatorController controller) {
        var o=new GameObject(name); o.AddComponent<SpriteRenderer>().sprite=sprite; o.AddComponent<Animator>().runtimeAnimatorController=controller;
        Body(o); o.AddComponent<CircleCollider2D>().radius=.32f; o.AddComponent<DepthSort>(); return o;
    }
    static Tile TileAsset(string name,Sprite sprite) { var t=ScriptableObject.CreateInstance<Tile>(); t.sprite=sprite; t.colliderType=Tile.ColliderType.None; AssetDatabase.CreateAsset(t,"Assets/Tilemaps/"+name+".asset"); return t; }
    static Tilemap Map(string name,Transform parent,int order) { var o=new GameObject(name); o.transform.SetParent(parent); var map=o.AddComponent<Tilemap>(); o.AddComponent<TilemapRenderer>().sortingOrder=order; return map; }
    [MenuItem("Crypt Survivor/Build initial arena")]
    public static void Build() {
        if(File.Exists(ScenePath)) { Debug.Log("GameArena já existe; geração automática preservou a cena."); return; }
        EditorSettings.defaultBehaviorMode=EditorBehaviorMode.Mode2D;
        var playerController=AnimatorController.CreateAnimatorControllerAtPath("Assets/Animations/Player.controller");
        Sprite[] idle=null;
        // Sheet rows: N, NW, W, SW, S, SE, E, NE. Runtime directions: E, NE, N, NW, W, SW, S, SE.
        int[] rows={6,7,0,1,2,3,4,5};
        foreach(string state in new[]{"Idle","Walk","Attack","Run","Roll"}) {
            int cell=state=="Attack"?96:state=="Roll"?48:64;
            var frames=Slice("Assets/Art/Player/"+state.ToLower()+".png",cell,cell,24);
            int columns=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Player/"+state.ToLower()+".png").width/cell;
            if(state=="Idle") idle=frames;
            for(int d=0;d<8;d++) State(playerController,state+d,Clip("Player_"+state+d,frames.Skip(rows[d]*columns).Take(columns).ToArray(),state=="Attack"?22:12,state!="Attack" && state!="Roll"));
        }
        var enemyController=AnimatorController.CreateAnimatorControllerAtPath("Assets/Animations/Skeleton.controller");
        Sprite[] skeleton=null;
        foreach(string state in new[]{"Idle","Walk","Death"}) {
            var frames=Slice("Assets/Art/Crypt/skeleton-variation1-"+state.ToLower()+".png",128,128,32);
            if(state=="Idle") skeleton=frames;
            State(enemyController,state,Clip("Skeleton_"+state,frames,10,state!="Death"));
        }
        var terrain=Slice("Assets/Art/Crypt/Tileset-Terrain.png",32,32,32);
        var walls=Slice("Assets/Art/Crypt/wall-1.png",32,32,32);
        var arrow=Slice("Assets/Art/Crypt/arrow travelling.png",39,48,32);
        var projectile=new GameObject("Projectile"); projectile.AddComponent<SpriteRenderer>().sprite=arrow[0]; projectile.GetComponent<SpriteRenderer>().sortingOrder=2000;
        Body(projectile); var pc=projectile.AddComponent<CircleCollider2D>(); pc.radius=.12f; pc.isTrigger=true; projectile.AddComponent<Projectile>();
        var projectilePrefab=PrefabUtility.SaveAsPrefabAsset(projectile,"Assets/Prefabs/Projectile.prefab"); UnityEngine.Object.DestroyImmediate(projectile);
        var player=Actor("Player",idle[0],playerController); player.tag="Player";
        player.AddComponent<PlayerMovement>(); player.AddComponent<PlayerAnimationController>(); player.AddComponent<PlayerHealth>();
        player.AddComponent<PlayerAttack>().ProjectilePrefab=projectilePrefab.GetComponent<Projectile>();
        var playerPrefab=PrefabUtility.SaveAsPrefabAsset(player,"Assets/Prefabs/Player.prefab"); UnityEngine.Object.DestroyImmediate(player);
        var enemy=Actor("Skeleton",skeleton[0],enemyController); enemy.AddComponent<EnemyController>();
        var enemyPrefab=PrefabUtility.SaveAsPrefabAsset(enemy,"Assets/Prefabs/Skeleton.prefab"); UnityEngine.Object.DestroyImmediate(enemy);
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Main Camera"); camera.tag="MainCamera"; var cam=camera.AddComponent<Camera>(); cam.orthographic=true; cam.orthographicSize=8.8f;
        camera.transform.position=new Vector3(0,0,-10); cam.clearFlags=CameraClearFlags.SolidColor; cam.backgroundColor=new Color(.015f,.025f,.04f); camera.AddComponent<AudioListener>();
        new GameObject("GameManager").AddComponent<GameManager>();
        var grid=new GameObject("Crypt Tilemaps"); grid.AddComponent<Grid>();
        var floor=Map("Floor",grid.transform,-2000); var wall=Map("Walls",grid.transform,1500);
        var floorTiles=new Tile[4]; for(int n=0;n<4;n++) floorTiles[n]=TileAsset("Floor"+n,terrain[9*50+17+n]);
        var wallTile=TileAsset("Wall",walls[10*16+1]);
        for(int x=-13;x<=12;x++) for(int y=-7;y<=6;y++) floor.SetTile(new Vector3Int(x,y,0),floorTiles[((x*13+y*7)&int.MaxValue)%4]);
        for(int x=-14;x<=13;x++) { wall.SetTile(new Vector3Int(x,7,0),wallTile); wall.SetTile(new Vector3Int(x,-8,0),wallTile); }
        for(int y=-7;y<=6;y++) { wall.SetTile(new Vector3Int(-14,y,0),wallTile); wall.SetTile(new Vector3Int(13,y,0),wallTile); }
        foreach(var spec in new[]{new Vector4(0,7.5f,28,1),new Vector4(0,-7.5f,28,1),new Vector4(-13.5f,0,1,14),new Vector4(13.5f,0,1,14)}) {
            var boundary=new GameObject("Solid crypt wall"); boundary.transform.SetParent(grid.transform); boundary.transform.position=new Vector3(spec.x,spec.y,0); boundary.AddComponent<BoxCollider2D>().size=new Vector2(spec.z,spec.w);
        }
        var coffin=Single("Assets/Art/Crypt/coffin - vertical - 1.png");
        for(int side=-1;side<=1;side+=2) for(int y=-4;y<=4;y+=4) {
            var prop=new GameObject("Crypt sarcophagus"); prop.transform.position=new Vector3(side*11.5f,y,0); prop.AddComponent<SpriteRenderer>().sprite=coffin; prop.AddComponent<DepthSort>();
        }
        var p=(GameObject)PrefabUtility.InstantiatePrefab(playerPrefab); p.transform.position=Vector3.zero;
        var spawnRoot=new GameObject("Enemy Spawn Points - Part 2");
        Vector2[] points={new Vector2(-9,4),new Vector2(9,4),new Vector2(-9,-4),new Vector2(9,-4),new Vector2(0,5)};
        foreach(var point in points) {
            var marker=new GameObject("Valid Spawn"); marker.transform.SetParent(spawnRoot.transform); marker.transform.position=point;
            var e=(GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab); e.transform.position=point; e.GetComponent<EnemyController>().Target=p.GetComponent<PlayerHealth>();
        }
        EditorSceneManager.SaveScene(scene,ScenePath);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        AssetDatabase.SaveAssets(); Selection.activeGameObject=p; SceneView.lastActiveSceneView?.FrameSelected();
        Debug.Log("CRYPT_ARENA_BUILD_OK");
    }
}


