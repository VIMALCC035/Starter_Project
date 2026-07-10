using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TreeSetup
{
    [MenuItem("Tools/Create Tree in Scene")]
    public static void CreateTreeInScene()
    {
        Scene scene = SceneManager.GetActiveScene();

        // Check if tree already exists
        if (GameObject.Find("Tree") != null)
        {
            EditorUtility.DisplayDialog("Tree Already Exists", "A tree already exists in the scene.", "OK");
            return;
        }

        // Create the tree
        TreeGenerator.CreateTree(Vector3.zero);

        // Mark scene as dirty
        EditorSceneManager.MarkSceneDirty(scene);
        EditorUtility.DisplayDialog("Success", "Tree created at position (0, 0, 0)", "OK");
    }
}
