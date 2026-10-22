using UnityEngine;
[RequireComponent(typeof(Rigidbody2D), typeof(EnemyHealth))]
public class EnemyController : MonoBehaviour
{
    public float MoveSpeed = 1.5f;
    public int ContactDamage = 10;
    public float ContactInterval = 1f;
    public PlayerHealth Target;
    Rigidbody2D body; SpriteRenderer sprite; Animator animator; float nextDamage; bool walking;
    void Awake() { body = GetComponent<Rigidbody2D>(); sprite = GetComponent<SpriteRenderer>(); animator = GetComponent<Animator>(); }
    void Start() { if(!Target) Target = FindObjectOfType<PlayerHealth>(); }
    void FixedUpdate() {
        Vector2 direction = Target && !Target.IsDead ? (Vector2)(Target.transform.position - transform.position) : Vector2.zero;
        body.velocity = direction.sqrMagnitude > .25f ? direction.normalized * MoveSpeed : Vector2.zero;
        if(direction.x != 0) sprite.flipX = direction.x < 0;
        bool moving = body.velocity.sqrMagnitude > .01f;
        if(moving != walking) { walking = moving; animator.Play(moving ? "Walk" : "Idle"); }
    }
    void OnCollisionStay2D(Collision2D collision) { Contact(collision.collider); }
    void OnCollisionEnter2D(Collision2D collision) { Contact(collision.collider); }
    void Contact(Collider2D other) {
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if(!enabled || GetComponent<EnemyHealth>().IsDead || !player || player.IsDead || Time.time < nextDamage) return;
        nextDamage = Time.time + ContactInterval; player.TakeDamage(ContactDamage);
    }
    void OnDisable() { if(body) body.velocity = Vector2.zero; }
}
