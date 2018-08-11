using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DemoWindowsEditor : EditorWindow
{

    /// <summary>
    /// 打开窗口，这种方式，失去Unity焦点，窗口依旧显示
    /// 快捷键 Ctrl + D，
    /// 快捷键符号 %=Ctrl #=Shift &=Alt
    /// </summary>
    [MenuItem("DemoEditor/OpenWin %D",priority = 1)]
    static void OpenWin()
    {
        // Get existing open window or if none, make a new one:
        var window = EditorWindow.GetWindow<DemoWindowsEditor>("DemosEditorWindows");
        window.minSize = new Vector2(300, 600);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Test"))
        {
            EditorUtility.DisplayDialog("提示确认", "确认", "OK");
        }
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// OnInspectorUpdate is called at 10 frames per second to give the inspector a chance to update.
    /// </summary>
	void OnInspectorUpdate()
    {
        // Call Repaint on OnInspectorUpdate as it repaints the windows
        // less times as if it was OnGUI/Update
        Repaint();
    }
}
