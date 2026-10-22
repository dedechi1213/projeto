using UnityEngine;
[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerAttack : MonoBehaviour
{
    public Projectile ProjectilePrefab;
    public float ProjectileSpeed = 12f;
    public int ProjectileDamage = 25;
    [Tooltip("Disparos por segundo")] public float FireRate = 4f;
    public float ProjectileLifetime = 2f;
    float nextShot;
    void Update() {
        if(!Input.GetMouseButton(0) || Time.time < nextShot || !Camera.main) return;
        Vector2 aim = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)transform.position;
        if(aim.sqrMagnitude < .001f) return;
        Fire(aim.normalized);
    }
    public void Fire(Vector2 direction) {
        if(!enabled || !ProjectilePrefab || direction.sqrMagnitude < .001f || Time.time < nextShot) return;
        direction.Normalize(); nextShot = Time.time + 1f / Mathf.Max(.1f, FireRate);
        Projectile shot = Instantiate(ProjectilePrefab, (Vector2)transform.position + direction * .55f, Quaternion.identity);
        shot.Launch(direction, ProjectileSpeed, ProjectileDamage, ProjectileLifetime, GetComponent<Collider2D>());
        GetComponent<PlayerAnimationController>().Attack(direction); GameAudio.Play(GameCue.Attack);
    }
}

