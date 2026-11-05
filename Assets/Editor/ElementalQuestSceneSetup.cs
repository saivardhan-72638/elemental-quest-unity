#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public static class ElementalQuestSceneSetup
{
    [MenuItem("Elemental Quest/Create / Reset Playable Scene")]
    public static void CreatePlayableScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        string folder = "Assets/Scenes";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.2f;
        camera.backgroundColor = new Color(0.08f, 0.12f, 0.2f);
        cameraObject.transform.position = new Vector3(0, 1.5f, -10);
        cameraObject.AddComponent<AudioListener>();

        GameObject bootstrap = new GameObject("Game Bootstrap");
        bootstrap.AddComponent<GameBootstrap>();

        string scenePath = folder + "/ElementalQuest.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Elemental Quest",
            "Playable scene created!\n\nOpen Assets/Scenes/ElementalQuest.unity and press Play.",
            "Let's Play");
    }
}
#endif
