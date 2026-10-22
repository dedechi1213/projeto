using UnityEngine;
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    public float MoveSpeed = 4.5f;
    public Vector2 MoveInput { get; private set; }
    Rigidbody2D body;
    void Awake() { body = GetComponent<Rigidbody2D>(); }
    void Update() { MoveInput = Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f); }
    void FixedUpdate() { body.velocity = MoveInput * MoveSpeed; }
    void OnDisable() { MoveInput = Vector2.zero; if(body) body.velocity = Vector2.zero; }
}
