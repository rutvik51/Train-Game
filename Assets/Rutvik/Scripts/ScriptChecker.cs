#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ScriptChecker : MonoBehaviour
{
    [MenuItem("Tools/Check and Remove Missing Scripts")]
    public static void CheckAndRemoveMissingScripts()
    {
        // Get all root objects in the scene
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        int totalMissingScripts = 0;

        foreach (GameObject rootObject in rootObjects)
        {
            // Check the root object and all its children
            totalMissingScripts += CheckAndRemoveMissingScriptsRecursively(rootObject);
        }

        Debug.Log($"Finished checking. Total missing scripts removed: {totalMissingScripts}");
    }

    private static int CheckAndRemoveMissingScriptsRecursively(GameObject obj)
    {
        int missingScriptsCount = 0;

        // Get all components on this GameObject
        Component[] components = obj.GetComponents<Component>();

        // Loop through each component and check for missing scripts
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                // A missing script was found, remove it
                Undo.RegisterCompleteObjectUndo(obj, "Remove missing script");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
                missingScriptsCount++;
            }
        }

        // Recursively check children
        foreach (Transform child in obj.transform)
        {
            missingScriptsCount += CheckAndRemoveMissingScriptsRecursively(child.gameObject);
        }

        return missingScriptsCount;
    }
}
#endif