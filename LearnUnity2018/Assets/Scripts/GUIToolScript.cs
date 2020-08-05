using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GUIToolScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        AssetBundle.UnloadAllAssetBundles(true);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public GameObject _last_test_mono_obj = null;
    AssetBundle _ab = null;
    Object _ab_obj = null;

    public GameObject _editor_load_go = null;
    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("AddTestMono"/*, GUILayout.Width(80)*/))
        {
            var go = new GameObject("TestMono");
            go.AddComponent<TestMonoScript>();
            go.AddComponent<TestMonoScript2>();
            Debug.Log("After AddTestMono");
            _last_test_mono_obj = go;
        }
        if (GUILayout.Button("InstanceMono"/*, GUILayout.Width(80)*/))
        {
            if(_last_test_mono_obj != null)
            {
                var go = GameObject.Instantiate<GameObject>(_last_test_mono_obj);
                Debug.Log("After InstanceMono");
                _last_test_mono_obj = go;
            }
        }
        if (GUILayout.Button("unloadasset"))
        {
            var root = GameObject.Find("Canvas");
            var img_tra = root.transform.Find("ImgContain12/Image1");
            var img = img_tra.GetComponent<UnityEngine.UI.Image>().sprite;
            if(img != null)
            {
                Resources.UnloadAsset(img);
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("bundle 1 loadfromfile"))
        {
            if(_ab == null)
                _ab = AssetBundle.LoadFromFile("AssetBundles/imgcontain12.prefab");
        }
        if (GUILayout.Button("bundle 2 load, conflict"))
        {
            var ab2 = AssetBundle.LoadFromFile("AssetBundles/all_2.ab");
            var ab3 = AssetBundle.LoadFromFile("AssetBundles/all_3.ab");
            // 冲突没有发生，实践证明，AssetBundle加载冲突是以当初打ab时的名字来比较的。
        }
        if (GUILayout.Button("bundle 2 loadasset"))
        {
            if (_ab != null) _ab_obj = _ab.LoadAllAssets()[0];
        }
        if (GUILayout.Button("bundle 3 instantiate"))
        {
            if (_ab_obj != null) GameObject.Instantiate(_ab_obj);
        }
        if (GUILayout.Button("bundle 3 unload"))
        {
            if(_ab != null)
            {
                _ab.Unload(true);
                if (_ab_obj != null) GameObject.Destroy(_ab_obj);
                _ab = null;
                _ab_obj = null;
            }
        }
        if (GUILayout.Button("bundle print name"))
        {
            AssetBundle[] bundles = Resources.FindObjectsOfTypeAll<AssetBundle>();
            Debug.Log("number of bundles " + bundles.Length);

            for (int i = 0; i < bundles.Length; i++)
            {
                Debug.Log("Bundle: " + bundles[i].name);
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("editor loadfromfile"))
        {
#if UNITY_EDITOR
            if (_editor_load_go == null)
            {
                _editor_load_go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ABMgr/Res/Prefab/ImgContain12.prefab");
            }
            GameObject.Instantiate(_editor_load_go,GameObject.Find("Canvas").transform);
#endif
        }
        GUILayout.EndHorizontal();
        ShowResolution();
    }

    void ShowResolution()
    {
        GUILayout.Label(Screen.width + "x" + Screen.height);
        GUILayout.Label(Screen.currentResolution.ToString());
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("add"))
        {
            Screen.SetResolution(Screen.width * 2, Screen.height * 2, true);
        }
        if (GUILayout.Button("mini"))
        {
            Screen.SetResolution(Screen.width / 2, Screen.height / 2, true);
        }
        GUILayout.EndHorizontal();
    }
}
