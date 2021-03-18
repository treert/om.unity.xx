using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(MonoDemo),true, isFallback = true)]
public class MonoDemoEditor: Editor
{
    [MenuItem("CONTEXT/MonoDemo/CallFunc_1", false, 9999)]
    static void CallFunc_1()
    {
        var obj = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Res/Img/40.gif");
        Debug.Log(obj);
        var objs = AssetDatabase.LoadAllAssetsAtPath("Assets/Res/Img/40.gif");
        foreach(var o  in objs)
        {
            Debug.Log(o);
        }
    }

    SerializedProperty m_obj;
    MonoDemo m_target;

    private void OnEnable()
    {
        m_obj = serializedObject.FindProperty("obj");
        m_target = target as MonoDemo;
    }

    void f()
    {
        
    }

    public override void OnInspectorGUI()
    {
        //EditorGUILayout.SelectableLabel("test");
        base.OnInspectorGUI();
        if (GUILayout.Button("test"))
        {
            var path = "Assets/Res/Atlas/AcFunMeme.spriteatlas";
            var deps = AssetDatabase.GetDependencies(path, false);
            Debug.Log(string.Join("\n", deps));
        }
    }
}
