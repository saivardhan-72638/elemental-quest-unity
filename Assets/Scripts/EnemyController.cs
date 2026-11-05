using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class EnemyController : MonoBehaviour
{
    public int health = 2;
    public bool IsDead => health <= 0;

    private Rigidbody2D body;
    private GameBootstrap game;
    private float freezeUntil;
    private float nextContactDamage;
    private float patrolDirection = 1f;
    private float patrolOrigin;
    private bool initialized;

    public void Initialize(GameBootstrap bootstrap)
    {
        game = bootstrap;
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 3f;
        body.freezeRotation = true;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        patrolOrigin = transform.position.x;
        initialized = true;
    }

    private void FixedUpdate()
    {
        if (!initialized || IsDead || game.IsPaused) return;
        if (Time.time < freezeUntil)
        {
            body.velocity = new Vector2(0, body.velocity.y);
            return;
        }

        if (Mathf.Abs(transform.position.x - patrolOrigin) > 2.2f)
            patrolDirection *= -1f;
        body.velocity = new Vector2(patrolDirection * 1.1f, body.velocity.y);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (IsDead || game == null || game.IsPaused) return;
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null && Time.time >= nextContactDamage)
        {
            player.TakeDamage(1);
            nextContactDamage = Time.time + 1.1f;
        }
    }

    public void Freeze(float duration)
    {
        if (IsDead) return;
        freezeUntil = Mathf.Max(freezeUntil, Time.time + duration);
        GetComponent<SpriteRenderer>().color = new Color(0.35f, 0.85f, 1f);
        CancelInvoke(nameof(ClearFreeze));
        Invoke(nameof(ClearFreeze), duration);
    }

    private void ClearFreeze()
    {
        if (!IsDead) GetComponent<SpriteRenderer>().color = new Color(0.75f, 0.2f, 0.25f);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        health -= amount;
        if (health <= 0)
        {
            Destroy(gameObject);
            if (game != null) game.AddScore(100);
        }
        else
        {
            GetComponent<SpriteRenderer>().color = new Color(1f, 0.65f, 0.25f);
            Invoke(nameof(ResetColor), 0.15f);
        }
    }

    private void ResetColor()
    {
        if (!IsDead && Time.time >= freezeUntil)
            GetComponent<SpriteRenderer>().color = new Color(0.75f, 0.2f, 0.25f);
    }
}
