using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 参考资料：
/// 1. Unity中常用的编辑器图形辅助线类 https://blog.csdn.net/Teddy_k/article/details/82117420
/// 
/// </summary>

[CustomEditor(typeof(DemoMonoScript))]
public class DemoInspectorEditor : Editor {

    DemoMonoScript myHandles;
    GUIStyle style;
    void OnEnable()
    {
        style = new GUIStyle();
        style.richText = true;
        style.fontSize = 20;
        style.alignment = TextAnchor.MiddleCenter;
        myHandles = (DemoMonoScript)target;
    }

    private void OnSceneGUI() // 选中时在Scene视图创建一个可调节Radius的Handle，调节后Player的radius值会改变
    {
        Handles.color = Color.white;
        myHandles.radius = UnityEditor.Handles.RadiusHandle(Quaternion.identity, myHandles.transform.position, myHandles.radius);
    }
}
