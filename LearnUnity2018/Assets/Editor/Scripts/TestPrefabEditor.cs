#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using System;

[InitializeOnLoad]
public class PrefabStageListener : Editor
{
    static PrefabStageListener()
    {
        // 打开Prefab编辑界面回调
        PrefabStage.prefabStageOpened += OnPrefabStageOpened;
        // Prefab被保存之前回调
        PrefabStage.prefabSaving += OnPrefabSaving;
        // Prefab被保存之后回调
        PrefabStage.prefabSaved += OnPrefabSaved;
        // 关闭Prefab编辑界面回调
        PrefabStage.prefabStageClosing += OnPrefabStageClosing;

        PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdated;
    }

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