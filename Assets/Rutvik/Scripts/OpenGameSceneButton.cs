#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class OpenGameSceneButton
{
    static OpenGameSceneButton()
    {
        ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
    }

    private static void OnToolbarGUI()
    {
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent("GAME SCENE", "Open Scene at index 0"), EditorStyles.toolbarButton))
        {
            OpenSceneAtIndex(0);
        }

        GUILayout.Space(10);

        if (GUILayout.Button(new GUIContent("CLEAR", "Clear all PlayerPrefs"), EditorStyles.toolbarButton))
        {
            ClearPlayerPrefs();
        }
    }

    private static void OpenSceneAtIndex(int index)
    {
        if (index < SceneManager.sceneCountInBuildSettings)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(index);
            if (!string.IsNullOrEmpty(scenePath))
            {
                EditorSceneManager.OpenScene(scenePath);
                Debug.Log($"Opened scene: {scenePath}");
            }
            else
            {
                Debug.LogWarning("Scene path is empty. Check the build settings.");
            }
        }
        else
        {
            Debug.LogWarning("Scene index out of range. Check the build settings.");
        }
    }

    private static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("All PlayerPrefs cleared.");
    }
}
#endif