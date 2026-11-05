using UnityEngine;

public class ElementalProjectile : MonoBehaviour
{
    private Vector2 direction;
    private GameBootstrap game;
    private float speed = 12f;
    private float lifetime = 2.2f;

    public void Initialize(Vector2 shotDirection, GameBootstrap bootstrap)
    {
        direction = shotDirection.normalized;
        game = bootstrap;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        transform.Rotate(0, 0, -720f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(1);
            Destroy(gameObject);
            return;
        }

        FireDestructible destructible = other.GetComponent<FireDestructible>();
        if (destructible != null)
        {
            destructible.Burn();
            Destroy(gameObject);
            return;
        }

        if (other.GetComponent<PlayerController>() == null)
            Destroy(gameObject);
    }
}
