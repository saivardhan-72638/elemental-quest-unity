using UnityEngine;

public enum ElementType { Fire, Ice, Wind }

public class ElementalAbilitySystem : MonoBehaviour
{
    public ElementType Selected { get; private set; } = ElementType.Fire;
    public float FireCooldown => Mathf.Max(0, fireReadyAt - Time.time);
    public float IceCooldown => Mathf.Max(0, iceReadyAt - Time.time);
    public float WindCooldown => Mathf.Max(0, windReadyAt - Time.time);

    private GameBootstrap game;
    private PlayerController player;
    private float fireReadyAt, iceReadyAt, windReadyAt;

    public void Initialize(GameBootstrap bootstrap, PlayerController controller)
    {
        game = bootstrap;
        player = controller;
    }

    public void Select(ElementType element)
    {
        Selected = element;
        if (game != null) game.RefreshHUD();
    }

    private void Update()
    {
        if (game == null || game.IsPaused || player == null || player.IsDead) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = ((Vector2)mouse - (Vector2)player.transform.position).normalized;
            if (direction.sqrMagnitude < 0.01f)
                direction = Vector2.right * player.FacingDirection;
            Activate(direction);
        }
    }

    public void Activate(Vector2 direction)
    {
        if (game == null || game.IsPaused || player == null || player.IsDead) return;
        direction = direction.sqrMagnitude < 0.01f
            ? Vector2.right * player.FacingDirection : direction.normalized;

        switch (Selected)
        {
            case ElementType.Fire:
                if (Time.time < fireReadyAt) { game.SetStatus("Fire is recharging!"); return; }
                fireReadyAt = Time.time + 1f;
                GameObject projectile = new GameObject("Fire Projectile");
                projectile.transform.position = player.transform.position + (Vector3)(direction * 0.75f);
                projectile.transform.localScale = new Vector3(0.42f, 0.42f, 1f);
                SpriteRenderer fireRenderer = projectile.AddComponent<SpriteRenderer>();
                fireRenderer.sprite = game.WhiteSprite;
                fireRenderer.color = new Color(1f, 0.35f, 0.08f);
                projectile.AddComponent<CircleCollider2D>().isTrigger = true;
                Rigidbody2D fireBody = projectile.AddComponent<Rigidbody2D>();
                fireBody.gravityScale = 0f;
                fireBody.bodyType = RigidbodyType2D.Kinematic;
                ElementalProjectile fire = projectile.AddComponent<ElementalProjectile>();
                fire.Initialize(direction, game);
                game.SetStatus("FIRE BURST!");
                break;

            case ElementType.Ice:
                if (Time.time < iceReadyAt) { game.SetStatus("Ice is recharging!"); return; }
                iceReadyAt = Time.time + 4f;
                EnemyController enemy = FindEnemyInDirection(direction, 3.2f, 0.75f);
                if (enemy != null)
                {
                    enemy.Freeze(2.5f);
                    game.SetStatus("Enemy frozen!");
                }
                else
                {
                    Vector2 platformPosition = (Vector2)player.transform.position + direction * 2.2f
                        + Vector2.down * 0.25f;
                    game.CreateIcePlatform(platformPosition);
                    game.SetStatus("Temporary ice platform created!");
                }
                break;

            case ElementType.Wind:
                if (Time.time < windReadyAt) { game.SetStatus("Wind is recharging!"); return; }
                windReadyAt = Time.time + 2f;
                Vector2 origin = player.transform.position;
                Collider2D[] hits = Physics2D.OverlapCircleAll(origin + direction * 1.2f, 2.0f);
                int pushed = 0;
                foreach (Collider2D hit in hits)
                {
                    if (hit.gameObject == player.gameObject) continue;
                    EnemyController targetEnemy = hit.GetComponent<EnemyController>();
                    Rigidbody2D targetBody = hit.attachedRigidbody;
                    if (targetEnemy != null || hit.GetComponent<PushableMarker>() != null)
                    {
                        if (targetBody != null)
                        {
                            Vector2 away = ((Vector2)hit.transform.position - origin).normalized;
                            targetBody.AddForce(away * 7f + Vector2.up * 1.5f, ForceMode2D.Impulse);
                            pushed++;
                        }
                    }
                }
                game.SetStatus(pushed > 0 ? "WIND GUST! Objects pushed." : "WIND GUST!");
                break;
        }
        game.RefreshHUD();
    }

    private EnemyController FindEnemyInDirection(Vector2 direction, float range, float dotThreshold)
    {
        EnemyController best = null;
        float bestDistance = range;
        foreach (EnemyController enemy in FindObjectsOfType<EnemyController>())
        {
            if (enemy.IsDead) continue;
            Vector2 offset = (Vector2)enemy.transform.position - (Vector2)player.transform.position;
            float distance = offset.magnitude;
            if (distance <= bestDistance && distance > 0.2f &&
                Vector2.Dot(direction, offset.normalized) >= dotThreshold)
            {
                best = enemy;
                bestDistance = distance;
            }
        }
        return best;
    }
}
