//-----------------------------------------------------------------------// <copyright file="AOTSupportScanner.cs" company="Sirenix IVS"> // Copyright (c) Sirenix IVS. All rights reserved.// </copyright>//-----------------------------------------------------------------------
#if UNITY_EDITOR

namespace Sirenix.Serialization.Editor
{
    using Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using System.Reflection;

    public sealed class AOTSupportScanner : IDisposable
    {
        private bool scanning;
        private bool allowRegisteringScannedTypes;
        private HashSet<Type> seenSerializedTypes = new HashSet<Type>();

        private static readonly MethodInfo PlayerSettings_GetPreloadedAssets_Method = typeof(PlayerSettings).GetMethod("GetPreloadedAssets", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);

        public void BeginScan()
        {
            this.scanning = true;
            allowRegisteringScannedTypes = false;

            this.seenSerializedTypes.Clear();

            FormatterLocator.OnLocatedEmittableFormatterForType += this.OnLocatedEmitType;
            FormatterLocator.OnLocatedFormatter += this.OnLocatedFormatter;
            Serializer.OnSerializedType += this.OnSerializedType;
        }

        public bool ScanPreloadedAssets(bool showProgressBar)
        {
            // The API does not exist in this version of Unity
            if (PlayerSettings_GetPreloadedAssets_Method == null) return true;

            UnityEngine.Object[] assets = (UnityEngine.Object[])PlayerSettings_GetPreloadedAssets_Method.Invoke(null, null);

            if (assets == null) return true;

            try
            {
                for (int i = 0; i < assets.Length; i++)
                {
                    if (showProgressBar && EditorUtility.DisplayCancelableProgressBar("Scanning preloaded assets for AOT support", (i + 1) + " / " + assets.Length, (float)i / assets.Length))
                    {
                        return false;
                    }

                    var asset = assets[i];

                    if (asset == null) continue;

                    if (AssetDatabase.Contains(asset))
                    {
                        // Scan the asset and all its dependencies
                        var path = AssetDatabase.GetAssetPath(asset);
                        this.ScanAsset(path, true);
                    }
                    else
                    {
                        // Just scan the asset
                        this.ScanObject(assets[i]);
                    }
                }
            }
            finally
            {
                if (showProgressBar)
                {
                    EditorUtility.ClearProgressBar();
                }
            }

            return true;
        }

        public bool ScanAssetBundle(string bundle)
        {
            string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
            
            foreach (var asset in assets)
            {
                this.ScanAsset(asset, true);
            }

            return true;
        }

        public bool ScanAllAssetBundles(bool showProgressBar)
        {
            try
            {
                string[] bundles = AssetDatabase.GetAllAssetBundleNames();

                for (int i = 0; i < bundles.Length; i++)
                {
                    var bundle = bundles[i];

                    if (showProgressBar && EditorUtility.DisplayCancelableProgressBar("Scanning asset bundles for AOT support", bundle, (float)i / bundles.Length))
                    {
                        return false;
                    }

                    this.ScanAssetBundle(bundle);
                }
            }
            finally
            {
                if (showProgressBar)
                {
                    EditorUtility.ClearProgressBar();
                }
            }

            return true;
        }

        public bool ScanAllResources(bool includeResourceDependencies, bool showProgressBar)
        {
            try
            {
                if (showProgressBar && EditorUtility.DisplayCancelableProgressBar("Scanning resources for AOT support", "Loading resource assets", 0f))
                {
                    return false;
                }

                var resources = Resources.LoadAll("");

                for (int i = 0; i < resources.Length; i++)
                {
                    if (showProgressBar && EditorUtility.DisplayCancelableProgressBar("Scanning resource " + i + " for AOT support", resources[i].name, (float)i / resources.Length))
                    {
                        return false;
                    }

                    var assetPath = AssetDatabase.GetAssetPath(resources[i]);

                    // Exclude editor-only resources
                    if (assetPath.ToLower().Contains("/editor/")) continue;

                    this.ScanAsset(assetPath, includeAssetDependencies: includeResourceDependencies);
                }

                return true;
            }
            finally
            {
                if (showProgressBar)
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        public bool ScanBuildScenes(bool includeSceneDependencies, bool showProgressBar)
        {
            var scenePaths = EditorBuildSettings.scenes
                .Where(n => n.enabled)
                .Select(n => n.path)
                .ToArray();

            return this.ScanScenes(scenePaths, includeSceneDependencies, showProgressBar);
        }

        public bool ScanScenes(string[] scenePaths, bool includeSceneDependencies, bool showProgressBar)
        {
            if (scenePaths.Length == 0) return true;

            bool formerForceEditorModeSerialization = UnitySerializationUtility.ForceEditorModeSerialization;

            try
            {
                UnitySerializationUtility.ForceEditorModeSerialization = true;

                bool hasDirtyScenes = false;

                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    if (EditorSceneManager.GetSceneAt(i).isDirty)
                    {
                        hasDirtyScenes = true;
                        break;
                    }
                }

                if (hasDirtyScenes && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return false;
                }

                var oldSceneSetup = EditorSceneManager.GetSceneManagerSetup();

                try
                {
                    for (int i = 0; i < scenePaths.Length; i++)
                    {
                        var scenePath = scenePaths[i];

                        if (showProgressBar && EditorUtility.DisplayCancelableProgressBar("Scanning scenes for AOT support", "Scene " + (i + 1) + "/" + scenePaths.Length + " - " + scenePath, (float)i / scenePaths.Length))
                        {
                            return false;
                        }

                        var openScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                        var sceneGOs = Resources.FindObjectsOfTypeAll<GameObject>();

                        foreach (var go in sceneGOs)
                        {
                            if (go.scene != openScene) continue;
                            
                            if ((go.hideFlags & HideFlags.DontSaveInBuild) == 0)
                            {
                                foreach (var component in go.GetComponents<ISerializationCallbackReceiver>())
                                {
                                    try
                                    {
                                        this.allowRegisteringScannedTypes = true;
                                        component.OnBeforeSerialize();

                                        var prefabSupporter = component as ISupportsPrefabSerialization;

                                        if (prefabSupporter != null)
                                        {
                                            // Also force a serialization of the object's prefab modifications, in case there are unknown types in there

                                            List<UnityEngine.Object> objs = null;
                                            var mods = UnitySerializationUtility.DeserializePrefabModifications(prefabSupporter.SerializationData.PrefabModifications, prefabSupporter.SerializationData.PrefabModificationsReferencedUnityObjects);
                                            UnitySerializationUtility.SerializePrefabModifications(mods, ref objs);
                                        }
                                    }
                                    finally
                                    {
                                        this.allowRegisteringScannedTypes = false;
                                    }
                                }
                            }
                        }
                    }

                    // Load a new empty scene that will be unloaded immediately, just to be sure we completely clear all changes made by the scan
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                }
                finally
                {
                    if (oldSceneSetup != null && oldSceneSetup.Length > 0)
                    {
                        if (showProgressBar)
                        {
                            EditorUtility.DisplayProgressBar("Restoring scene setup", "", 1.0f);
                        }
                        EditorSceneManager.RestoreSceneManagerSetup(oldSceneSetup);
                    }
                }

                if (includeSceneDependencies)
                {
                    for (int i = 0; i < scenePaths.Length; i++)
                    {
                        var scenePath = scenePaths[i];
                        if (showProgressBar && EditorUtility.DisplayCancelableProgressBar("Scanning scene dependencies for AOT support", "Scene " + (i + 1) + "/" + scenePaths.Length + " - " + scenePath, (float)i / scenePaths.Length))
                        {
                            return false;
                        }

                        string[] dependencies = AssetDatabase.GetDependencies(scenePath, recursive: true);

                        foreach (var dependency in dependencies)
                        {
                            this.ScanAsset(dependency, includeAssetDependencies: false); // All dependencies of this asset were already included recursively by Unity
                        }
                    }
                }

                return true;
            }
            finally
            {
                if (showProgressBar)
                {
                    EditorUtility.ClearProgressBar();
                }

                UnitySerializationUtility.ForceEditorModeSerialization = formerForceEditorModeSerialization;
            }
        }

        public bool ScanAsset(string assetPath, bool includeAssetDependencies)
        {
            if (!(assetPath.EndsWith(".asset") || assetPath.EndsWith(".prefab")))
            {
                // ScanAsset can only scan .asset and .prefab assets.
                return false;
            }

            bool formerForceEditorModeSerialization = UnitySerializationUtility.ForceEditorModeSerialization;

            try
            {
                UnitySerializationUtility.ForceEditorModeSerialization = true;

                var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

                if (assets == null || assets.Length == 0)
                {
                    return false;
                }

                foreach (var asset in assets)
                {
                    this.ScanObject(asset);
                }

                if (includeAssetDependencies)
                {
                    string[] dependencies = AssetDatabase.GetDependencies(assetPath, recursive: true);

                    foreach (var dependency in dependencies)
                    {
                        this.ScanAsset(dependency, includeAssetDependencies: false); // All dependencies were already included recursively by Unity
                    }
                }

                return true;
            }
            finally
            {
                UnitySerializationUtility.ForceEditorModeSerialization = formerForceEditorModeSerialization;
            }
        }

        public void ScanObject(UnityEngine.Object obj)
        {
            if (obj is ISerializationCallbackReceiver)
            {
                bool formerForceEditorModeSerialization = UnitySerializationUtility.ForceEditorModeSerialization;

                try
                {
                    UnitySerializationUtility.ForceEditorModeSerialization = true;
                    this.allowRegisteringScannedTypes = true;
                    (obj as ISerializationCallbackReceiver).OnBeforeSerialize();
                }
                finally
                {
                    this.allowRegisteringScannedTypes = false;
                    UnitySerializationUtility.ForceEditorModeSerialization = formerForceEditorModeSerialization;
                }
            }
        }

        public List<Type> EndScan()
        {
            if (!this.scanning) throw new InvalidOperationException("Cannot end a scan when scanning has not begun.");

            var result = this.seenSerializedTypes.ToList();
            this.Dispose();
            return result;
        }

        public void Dispose()
        {
            if (this.scanning)
            {
                FormatterLocator.OnLocatedEmittableFormatterForType -= this.OnLocatedEmitType;
                FormatterLocator.OnLocatedFormatter -= this.OnLocatedFormatter;
                Serializer.OnSerializedType -= this.OnSerializedType;

                this.scanning = false;
                this.seenSerializedTypes.Clear();
                this.allowRegisteringScannedTypes = false;
            }
        }

        private void OnLocatedEmitType(Type type)
        {
            if (!AllowRegisterType(type)) return;

            this.RegisterType(type);
        }

        private void OnSerializedType(Type type)
        {
            if (!AllowRegisterType(type)) return;

            this.RegisterType(type);
        }

        private void OnLocatedFormatter(IFormatter formatter)
        {
            var type = formatter.SerializedType;

            if (type == null) return;
            if (!AllowRegisterType(type)) return;
            this.RegisterType(type);
        }

        private static bool AllowRegisterType(Type type)
        {
            var typeFlags = AssemblyUtilities.GetAssemblyTypeFlag(type.Assembly);
            if ((typeFlags & AssemblyTypeFlags.UnityEditorTypes) == AssemblyTypeFlags.UnityEditorTypes)
            {
                return false;
            }

            if ((typeFlags & AssemblyTypeFlags.UserEditorTypes) == AssemblyTypeFlags.UserEditorTypes)
            {
                return false;
            }

            if (type.IsGenericType)
            {
                foreach (var parameter in type.GetGenericArguments())
                {
                    if (!AllowRegisterType(parameter)) return false;
                }
            }

            return true;
        }

        private void RegisterType(Type type)
        {
            if (!this.allowRegisteringScannedTypes) return;
            if (type.IsAbstract || type.IsInterface) return;
            if (type.IsGenericType && (type.IsGenericTypeDefinition || !type.IsFullyConstructedGenericType())) return;

            //if (this.seenSerializedTypes.Add(type))
            //{
            //    Debug.Log("Added " + type.GetNiceFullName());
            //}

            this.seenSerializedTypes.Add(type);

            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                {
                    this.RegisterType(arg);
                }
            }
        }
    }
}

#endif