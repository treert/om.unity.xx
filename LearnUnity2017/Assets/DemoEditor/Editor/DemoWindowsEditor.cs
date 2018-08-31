﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

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

    List<MethodInfo> m_test_methods = new List<MethodInfo>();
    public DemoWindowsEditor()
    {
        var cls = typeof(DemoWindowsEditor);
        var methods = cls.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic| BindingFlags.Instance);
        foreach(var m in methods)
        {
            if (m.Name.StartsWith("Test"))
            {
                m_test_methods.Add(m);
            }
        }
    }

    private void OnGUI()
    {
        AddUIToToolBar();

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Test"))
        {
            EditorUtility.DisplayDialog("提示确认", "确认", "OK");
        }
        GUILayout.EndHorizontal();

        
        if(m_test_methods.Count > 0)
        {
            foreach(var m in m_test_methods)
            {
                m.Invoke(this, new object[] { });
            }
        }
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

    void AddUIToToolBar()
    {
        //tool bar
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        {
            if (GUILayout.Button("Add", EditorStyles.toolbarButton))
            {

            }
            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
            {
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Build", EditorStyles.toolbarButton))
            {
            }
        }
        GUILayout.EndHorizontal();
    }

    void TestShowNotification()
    {
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("TestShowNotification"))
        {
            ShowNotification(new GUIContent("提示一行字"));
        }
        GUILayout.EndHorizontal();
    }
}
