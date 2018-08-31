using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ABBuild : EditorWindow {

    [MenuItem("ABMgr/OpenABBuild", priority = 1)]
    static void OpenWin()
    {
        // Get existing open window or if none, make a new one:
        var window = EditorWindow.GetWindow<ABBuild>("ABBuild");
        window.minSize = new Vector2(300, 600);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Build",GUILayout.Width(100)))
        {
            EditorUtility.DisplayDialog("提示确认", "确认", "OK");
        }
        GUILayout.EndHorizontal();
    }
}
