using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float jumpForce = 13f;
    public int maxHealth = 5;

    public int Health { get; private set; }
    public int FacingDirection { get; private set; } = 1;
    public bool IsGrounded { get; private set; }
    public bool IsDead => Health <= 0;

    private Rigidbody2D body;
    private Collider2D bodyCollider;
    private float horizontal;
    private float nextDamageTime;
    private bool jumpQueued;
    private GameBootstrap game;

    public void Initialize(GameBootstrap bootstrap)
    {
        game = bootstrap;
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        Health = maxHealth;
        body.freezeRotation = true;
        body.gravityScale = 3f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        if (game == null || game.IsPaused || IsDead) return;

        horizontal = Input.GetAxisRaw("Horizontal");
        if (horizontal > 0.01f) FacingDirection = 1;
        else if (horizontal < -0.01f) FacingDirection = -1;

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded)
            jumpQueued = true;

        if (Input.GetKeyDown(KeyCode.Alpha1)) game.Abilities.Select(ElementType.Fire);
        if (Input.GetKeyDown(KeyCode.Alpha2)) game.Abilities.Select(ElementType.Ice);
        if (Input.GetKeyDown(KeyCode.Alpha3)) game.Abilities.Select(ElementType.Wind);
    }

    private void FixedUpdate()
    {
        if (body == null || game == null || game.IsPaused || IsDead) return;

        body.velocity = new Vector2(horizontal * moveSpeed, body.velocity.y);
        if (jumpQueued)
        {
            body.velocity = new Vector2(body.velocity.x, jumpForce);
            jumpQueued = false;
            IsGrounded = false;
        }

        Vector2 check = new Vector2(bodyCollider.bounds.center.x,
            bodyCollider.bounds.min.y - 0.06f);
        IsGrounded = Physics2D.OverlapBox(check, new Vector2(bodyCollider.bounds.size.x * 0.75f, 0.12f),
            0f, LayerMask.GetMask("Default")) != null;
    }

    public void TakeDamage(int amount)
    {
        if (Time.time < nextDamageTime || IsDead) return;
        Health = Mathf.Max(0, Health - amount);
        nextDamageTime = Time.time + 1.0f;
        if (IsDead) game.ShowGameOver();
    }

    public void AddImpulse(Vector2 impulse)
    {
        if (body != null && !IsDead) body.AddForce(impulse, ForceMode2D.Impulse);
    }
}
