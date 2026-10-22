using UnityEngine;
[RequireComponent(typeof(Animator), typeof(PlayerMovement))]
public class PlayerAnimationController : MonoBehaviour
{
    Animator animator; PlayerMovement movement; int direction = 4; float attackUntil; string current;
    void Awake() { animator = GetComponent<Animator>(); movement = GetComponent<PlayerMovement>(); }
    public void Attack(Vector2 aim) { direction = Direction(aim); attackUntil = Time.time + .32f; Play("Attack", true); }
    public static int Direction(Vector2 v) { return ((Mathf.RoundToInt(Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg / 45f) + 8) % 8); }
    void LateUpdate() {
        if(Time.time < attackUntil) return;
        if(movement.MoveInput.sqrMagnitude > .01f) direction = Direction(movement.MoveInput);
        Play(movement.MoveInput.sqrMagnitude > .01f ? "Walk" : "Idle", false);
    }
    void Play(string state, bool restart) { string key = state + direction; if(restart || key != current) { animator.Play(key, 0, 0); current = key; } }
}
