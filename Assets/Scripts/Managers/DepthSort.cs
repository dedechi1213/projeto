using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class DepthSort : MonoBehaviour
{
    public int Offset;
    SpriteRenderer sprite;
    void Awake() { sprite = GetComponent<SpriteRenderer>(); }
    void LateUpdate() { sprite.sortingOrder = Offset - Mathf.RoundToInt(transform.position.y * 100); }
}
