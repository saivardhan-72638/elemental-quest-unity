using UnityEngine;

public class TemporaryPlatform : MonoBehaviour
{
    private float life = 6f;

    public void Initialize(float duration)
    {
        life = duration;
        Destroy(gameObject, life);
    }

    private void Update()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(0.45f, 1f, Mathf.PingPong(Time.time * 1.5f, 1f));
            sr.color = c;
        }
    }
}
