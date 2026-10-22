using UnityEngine;
public enum GameCue { Attack, Damage, EnemyDeath, Victory, Defeat }
[RequireComponent(typeof(AudioSource))]
public class GameAudio : MonoBehaviour
{
    static GameAudio instance;
    AudioSource source;
    readonly AudioClip[] clips = new AudioClip[5];
    void Awake() {
        instance=this; source=GetComponent<AudioSource>(); source.playOnAwake=false; source.spatialBlend=0; source.volume=.16f;
        clips[0]=Tone("Attack",new[]{720f,420f},.08f);
        clips[1]=Tone("Damage",new[]{150f,80f},.16f);
        clips[2]=Tone("Skeleton death",new[]{340f,190f,90f},.20f);
        clips[3]=Tone("Victory",new[]{392f,494f,587f,784f},.64f);
        clips[4]=Tone("Defeat",new[]{294f,247f,196f,147f},.70f);
    }
    static AudioClip Tone(string name,float[] notes,float duration) {
        const int rate=22050; var samples=new float[Mathf.RoundToInt(rate*duration)];
        float noteLength=duration/notes.Length;
        for(int n=0;n<samples.Length;n++) {
            float t=(float)n/rate; int note=Mathf.Min(notes.Length-1,(int)(t/noteLength));
            float progress=(t-note*noteLength)/noteLength;
            samples[n]=Mathf.Sin(2*Mathf.PI*notes[note]*t)*Mathf.Sin(Mathf.PI*progress)*.65f;
        }
        var clip=AudioClip.Create(name,samples.Length,1,rate,false); clip.SetData(samples,0); return clip;
    }
    public static void Play(GameCue cue) { if(instance && instance.source) instance.source.PlayOneShot(instance.clips[(int)cue]); }
    void OnDestroy() { if(instance==this) instance=null; foreach(var clip in clips) if(clip) Destroy(clip); }
}
