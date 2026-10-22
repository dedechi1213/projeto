using UnityEngine;
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    int damage; bool spent;
    public void Launch(Vector2 direction, float speed, int amount, float lifetime, Collider2D owner) {
        damage = amount; if(owner) Physics2D.IgnoreCollision(GetComponent<Collider2D>(), owner);
        transform.up = -direction; GetComponent<Rigidbody2D>().velocity = direction * speed;
        Destroy(gameObject, Mathf.Max(.05f, lifetime));
    }
    void OnTriggerEnter2D(Collider2D other) {
        if(spent || other.GetComponent<PlayerHealth>() || other.GetComponent<Projectile>() || other.isTrigger) return;
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if(enemy) { if(enemy.IsDead) return; enemy.TakeDamage(damage); }
        spent = true; Destroy(gameObject);
    }
}

