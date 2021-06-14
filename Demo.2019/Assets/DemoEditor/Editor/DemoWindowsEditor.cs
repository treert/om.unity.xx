using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditorInternal;

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
        window.minSize = new Vector2(800, 600);
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

    void TestDownList()
    {
        if (XEditorTools.DrawHeader("TestDownList"))
        {
            EditorGUILayout.PrefixLabel("AnyThing");
            EditorGUILayout.RectField("RectField", new Rect(1, 2, 3, 4));
        }
    }

    ReorderableList _reorder_list;
    void TestReorderableList()
    {
        if (XEditorTools.DrawHeader("TestReorderableList") == false)
        {
            return;
        }
        XEditorTools.BeginContents();
        if (_reorder_list == null)
        {
            List<int> list = new List<int>() {1,2,3,4,5 };
            _reorder_list = new ReorderableList(list, typeof(string));
            _reorder_list.draggable = true;
            _reorder_list.elementHeight = 22;
            _reorder_list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "header rect=" + rect.ToString());
            };
            _reorder_list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _reorder_list.list[index];
                var str = string.Format("rect={0} index={1} isActive={2} isFocused={3} value={4} list={5}", rect, index, isActive, isFocused, element, list[index]);
                //EditorGUILayout.PrefixLabel(str);
                Rect r = rect;
                r.width = 16;
                EditorGUI.Toggle(r, index > 1);
                r.x += 18;
                r.width = rect.width - 18;
                EditorGUI.TextField(r, str);

            };
        }
        _reorder_list.DoLayoutList();
        XEditorTools.EndContents();
    }

    void TestDisplayDialog()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Test"))
        {
            EditorUtility.DisplayDialog("提示确认", "确认", "OK");
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
