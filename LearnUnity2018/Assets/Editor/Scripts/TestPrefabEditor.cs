#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using System;

public class TestPrefabEditor : EditorWindow
{
    [MenuItem("OM/TestPrefabEditor", priority = 1)]
    static void OpenWin()
    {
        // Get existing open window or if none, make a new one:
        var window = EditorWindow.GetWindow<TestPrefabEditor>("TestPrefabEditor");
        window.minSize = new Vector2(300, 600);
        window.Show();
    }

    private void OnGUI()
    {
        go = EditorGUILayout.ObjectField(go, typeof(GameObject), true) as GameObject;
        EditorGUILayout.TextField("go.id", $"{(go ? go.GetInstanceID() : 0)}");
        ShowBtn(CreatePrefab);
        ShowBtn(LoadPrefab);
        ShowBtn(EditorPrefab);
        ShowBtn(SavePrefab);
        ShowBtn(RevertPrefab);
        ShowBtn(OverwritePrefab);

        GUILayout.Space(20);
        ShowBtn(DestroyGo);
        ShowBtn(DestroyImmediateGo);
        ShowBtn(DestroyImmediateGoWithAsset);
    }

    void ShowBtn(Action action)
    {
        if (GUILayout.Button(action.Method.Name, GUILayout.Width(200)))
        {
            action();
            //EditorUtility.DisplayDialog("提示确认", "确认", "OK");
        }
    }

    string test_path = "Assets/Temp/test.prefab";
    GameObject go;
    void CreatePrefab()
    {
        go = new GameObject("CreatePrefab");
        PrefabUtility.SaveAsPrefabAsset(go, test_path);
    }

    void LoadPrefab()
    {
        go = AssetDatabase.LoadAssetAtPath<GameObject>(test_path);
    }

    void UnloadPrefab()
    {
        Resources.UnloadAsset(go);
    }

    void InstancePrefab()
    {
        if (go)
        {
            go = GameObject.Instantiate<GameObject>(go);
        }
    }

    void EditorPrefab()
    {
        if (go)
        {
            go.transform.position = go.transform.position + Vector3.one;
        }
    }

    void SavePrefab()
    {
        if (go)
        {
            PrefabUtility.SavePrefabAsset(go);
        }
    }

    void RevertPrefab()
    {
        if (go)
        {
            // error 看来只操作Instance之后的
            PrefabUtility.RevertPrefabInstance(go, InteractionMode.AutomatedAction);
        }
    }

    void DestroyImmediateGo()
    {
        if (go)
        {
            // AssetDatabase 加载的资源用这个会报错：Destroying assets is not permitted to avoid data loss.
            GameObject.DestroyImmediate(go);
        }
    }

    void DestroyImmediateGoWithAsset()
    {
        if (go)
        {
            // 不会删掉文件。只是把Prefab里的
            GameObject.DestroyImmediate(go, true);
        }
    }

    void DestroyGo()
    {
        if (go)
        {
            // error, Destroy may not be called from edit mode! Use DestroyImmediate instead.
            GameObject.Destroy(go);
        }
    }

    void OverwritePrefab()
    {
        go = new GameObject("OverwritePrefab");
        PrefabUtility.SaveAsPrefabAssetAndConnect(go, test_path, InteractionMode.AutomatedAction);
    }
}


[InitializeOnLoad]
public class PrefabStageListener : Editor
{
    //static PrefabStageListener()
    //{
    //    // 打开Prefab编辑界面回调
    //    PrefabStage.prefabStageOpened += OnPrefabStageOpened;
    //    // Prefab被保存之前回调
    //    PrefabStage.prefabSaving += OnPrefabSaving;
    //    // Prefab被保存之后回调
    //    PrefabStage.prefabSaved += OnPrefabSaved;
    //    // 关闭Prefab编辑界面回调
    //    PrefabStage.prefabStageClosing += OnPrefabStageClosing;

    //    PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdated;
    //}

    private static void OnPrefabInstanceUpdated(GameObject instance)
    {
        Debug.Log(nameof(OnPrefabInstanceUpdated));
    }

    private static void OnPrefabStageClosing(PrefabStage obj)
    {
        Debug.Log(nameof(OnPrefabStageClosing));
    }

    private static void OnPrefabSaved(GameObject obj)
    {
        Debug.Log(nameof(OnPrefabSaved));
    }

    private static void OnPrefabSaving(GameObject obj)
    {
        Debug.Log(nameof(OnPrefabSaving));
    }

    private static void OnPrefabStageOpened(PrefabStage obj)
    {
        Debug.Log(nameof(OnPrefabStageOpened));
    }
}
#endif