﻿using System;
using System.IO;
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
            TestBuild();
            EditorUtility.DisplayDialog("提示确认", "确认", "OK");
        }
        GUILayout.EndHorizontal();
    }

    void TestBuild()
    {
        List<AssetBundleBuild> list = new List<AssetBundleBuild>();
        EnumObjectsFromDir("Assets/ABMgr/Res", (path) => {
            AddBuild(list, path, Path.GetFileName(path));
        });
        Directory.CreateDirectory("AssetBundles");
        var meta = BuildPipeline.BuildAssetBundles("AssetBundles",list.ToArray(), BuildAssetBundleOptions.UncompressedAssetBundle, EditorUserBuildSettings.activeBuildTarget);
        //string ab_path = "AssetBundles/imgcontain12.prefab";
        //Hash128 hash1 = new Hash128();
        //if(BuildPipeline.GetHashForAssetBundle(ab_path, out hash1))
        //{
        //}
        //AssetBundle.UnloadAllAssetBundles(true);
        //var ab = AssetBundle.LoadFromFile("AssetBundles/imgcontain12.prefab");
        //var objs = ab.LoadAllAssets();

        //Hash128 hash2 = meta.GetAssetBundleHash("imgcontain12.prefab");

        //var abs = meta.GetAllAssetBundles();
    }

    void EnumObjectsFromDir(string dir, Action<string> cb)
    {
        var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
        foreach(var f in files)
        {
            if (f.EndsWith(".meta") == false)
            {
                cb(f.Replace('\\','/'));
            }
        }
    }

    void  AddBuild(List<AssetBundleBuild> list, string path, string bundle_name)
    {
        AssetBundleBuild ret = new AssetBundleBuild();
        ret.assetBundleName = bundle_name;
        ret.assetNames = new string[] { path };
        list.Add(ret);
    }
}