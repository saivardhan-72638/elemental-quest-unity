using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    public Sprite WhiteSprite { get; private set; }
    public PlayerController Player { get; private set; }
    public ElementalAbilitySystem Abilities { get; private set; }
    public bool IsPaused { get; private set; }
    public int Score { get; private set; }

    private string status = "Find your path. Switch elements to overcome challenges.";
    private bool gameOver;
    private GUIStyle titleStyle, bodyStyle, smallStyle, buttonStyle;
    private float worldLeft = -11f;

    private void Awake()
    {
        CreateWhiteSprite();
        SetupCamera();
        BuildLevel();
        SetupPlayer();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !gameOver)
        {
            IsPaused = !IsPaused;
            status = IsPaused ? "Paused — press Escape to continue." : "Back to the quest!";
        }
    }

    private void CreateWhiteSprite()
    {
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
        texture.Apply();
        WhiteSprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cam = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }
        cam.orthographic = true;
        cam.orthographicSize = 5.2f;
        cam.backgroundColor = new Color(0.08f, 0.12f, 0.2f);
        cam.transform.position = new Vector3(0, 1.5f, -10);
    }

    private GameObject MakeBlock(string name, Vector2 position, Vector2 size, Color color,
        bool solid = true, string tagName = null)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = position;
        obj.transform.localScale = size;
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = WhiteSprite;
        sr.color = color;
        if (solid)
        {
            BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
        }
        if (!string.IsNullOrEmpty(tagName)) obj.tag = tagName;
        return obj;
    }

    private void BuildLevel()
    {
        MakeBlock("Ground", new Vector2(0, -3.5f), new Vector2(30, 1), new Color(0.18f, 0.25f, 0.32f));
        MakeBlock("Platform A", new Vector2(-5.4f, -1.7f), new Vector2(3.5f, 0.35f), new Color(0.28f, 0.42f, 0.48f));
        MakeBlock("Platform B", new Vector2(-0.2f, 0.0f), new Vector2(3.2f, 0.35f), new Color(0.28f, 0.42f, 0.48f));
        MakeBlock("Platform C", new Vector2(5.0f, -1.0f), new Vector2(3.4f, 0.35f), new Color(0.28f, 0.42f, 0.48f));
        MakeBlock("Wall", new Vector2(9.2f, -1.6f), new Vector2(0.5f, 3.8f), new Color(0.25f, 0.34f, 0.43f));

        GameObject fireBarrier = MakeBlock("Fire Barrier", new Vector2(-7.4f, -2.45f),
            new Vector2(0.45f, 1.1f), new Color(0.85f, 0.3f, 0.08f));
        fireBarrier.AddComponent<FireDestructible>();
        MakeBlock("Ice Marker", new Vector2(0.3f, 0.42f), new Vector2(0.8f, 0.12f), new Color(0.25f, 0.75f, 0.95f), false);
        GameObject crate = MakeBlock("Wind Crate", new Vector2(6.2f, -2.55f),
            new Vector2(0.8f, 0.8f), new Color(0.65f, 0.45f, 0.22f));
        crate.AddComponent<PushableMarker>();
        crate.AddComponent<Rigidbody2D>().gravityScale = 2.5f;

        CreateEnemy(new Vector2(-2.6f, -2.75f));
        CreateEnemy(new Vector2(4.4f, -0.45f));
        CreateDecor();
    }

    private void CreateDecor()
    {
        for (int i = 0; i < 12; i++)
        {
            float x = -10f + i * 1.8f;
            MakeBlock("Distant star", new Vector2(x, 3.2f + Mathf.Sin(i * 1.7f) * 0.8f),
                new Vector2(0.07f, 0.07f), new Color(0.9f, 0.85f, 0.65f), false);
        }
        MakeBlock("Level title accent", new Vector2(-7.8f, 3.8f), new Vector2(2.3f, 0.08f),
            new Color(1f, 0.55f, 0.15f), false);
    }

    private void CreateEnemy(Vector2 position)
    {
        GameObject enemy = MakeBlock("Elemental Slime", position, new Vector2(0.8f, 0.8f),
            new Color(0.75f, 0.2f, 0.25f));
        enemy.AddComponent<Rigidbody2D>();
        enemy.AddComponent<EnemyController>().Initialize(this);
    }

    private void SetupPlayer()
    {
        GameObject playerObject = MakeBlock("Player", new Vector2(-8.6f, -2.45f),
            new Vector2(0.65f, 0.95f), new Color(0.95f, 0.82f, 0.55f), false);
        playerObject.tag = "Player";
        Rigidbody2D rb = playerObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        CapsuleCollider2D collider = playerObject.AddComponent<CapsuleCollider2D>();
        collider.size = new Vector2(0.8f, 1f);
        Player = playerObject.AddComponent<PlayerController>();
        Player.Initialize(this);
        Abilities = playerObject.AddComponent<ElementalAbilitySystem>();
        Abilities.Initialize(this, Player);
    }

    public void CreateIcePlatform(Vector2 position)
    {
        GameObject platform = MakeBlock("Temporary Ice Platform", position,
            new Vector2(2.0f, 0.28f), new Color(0.3f, 0.82f, 1f, 0.85f));
        platform.AddComponent<TemporaryPlatform>().Initialize(6f);
    }

    public void AddScore(int points) { Score += points; }

    public void SetStatus(string message) { status = message; }

    public void RefreshHUD() { }

    public void ShowGameOver()
    {
        gameOver = true;
        status = "Quest over — press the Play button in Unity to restart.";
    }

    private void SetupHUDStyles()
    {
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
        titleStyle.normal.textColor = Color.white;
        bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold };
        bodyStyle.normal.textColor = Color.white;
        smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
        smallStyle.normal.textColor = new Color(0.82f, 0.88f, 0.95f);
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 14, fontStyle = FontStyle.Bold };
    }

    private void OnGUI()
    {
        if (titleStyle == null) SetupHUDStyles();
        float scale = Mathf.Clamp(Screen.width / 1100f, 0.75f, 1.25f);
        Matrix4x4 old = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1));
        float width = Screen.width / scale;
        GUI.Box(new Rect(14, 14, 390, 118), GUIContent.none);
        GUI.Label(new Rect(28, 20, 350, 32), "ELEMENTAL QUEST", titleStyle);
        GUI.Label(new Rect(28, 55, 350, 22), "HP: " + (Player != null ? Player.Health : 0) + "     SCORE: " + Score, bodyStyle);
        GUI.Label(new Rect(28, 82, 360, 22), "1 FIRE     2 ICE     3 WIND", smallStyle);

        GUI.Box(new Rect(width - 330, 14, 315, 105), GUIContent.none);
        GUI.Label(new Rect(width - 315, 22, 285, 22), "CONTROLS", bodyStyle);
        GUI.Label(new Rect(width - 315, 49, 285, 55), "A/D or Arrows: Move   Space: Jump\n1/2/3: Switch   Left Click: Use ability\nEsc: Pause", smallStyle);

        if (Abilities != null)
        {
            string selected = Abilities.Selected.ToString().ToUpper();
            float cooldown = Abilities.Selected == ElementType.Fire ? Abilities.FireCooldown :
                Abilities.Selected == ElementType.Ice ? Abilities.IceCooldown : Abilities.WindCooldown;
            GUI.Box(new Rect(14, 140, 310, 54), GUIContent.none);
            GUI.Label(new Rect(28, 148, 280, 22), "SELECTED: " + selected, bodyStyle);
            GUI.Label(new Rect(28, 172, 280, 18), cooldown <= 0 ? "READY — Left click to cast" :
                "COOLDOWN: " + cooldown.ToString("0.0") + "s", smallStyle);
        }

        GUI.Box(new Rect(14, Screen.height / scale - 56, width - 28, 40), GUIContent.none);
        GUI.Label(new Rect(28, Screen.height / scale - 48, width - 56, 24), status, smallStyle);

        if (IsPaused || gameOver)
        {
            GUI.Box(new Rect(width / 2 - 155, Screen.height / scale / 2 - 65, 310, 130), GUIContent.none);
            GUI.Label(new Rect(width / 2 - 125, Screen.height / scale / 2 - 48, 250, 30),
                gameOver ? "GAME OVER" : "PAUSED", titleStyle);
            if (GUI.Button(new Rect(width / 2 - 85, Screen.height / scale / 2 + 5, 170, 38),
                gameOver ? "Restart in Editor" : "Resume", buttonStyle))
            {
                if (!gameOver) IsPaused = false;
                else status = "Stop and Play again in Unity to restart.";
            }
        }
        GUI.matrix = old;
    }
}
