using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class DamageFlash : MonoBehaviour
{
    SpriteRenderer sprite; Color original; float until;
    void Awake() { sprite = GetComponent<SpriteRenderer>(); original = sprite.color; }
    public void Flash() { until = Time.time + .14f; sprite.color = new Color(1f,.25f,.25f); }
    void Update() { if(Time.time >= until) sprite.color = original; }
    void OnDisable() { if(sprite) sprite.color = original; }
}
