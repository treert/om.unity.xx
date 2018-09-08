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

    GameObject _last_test_mono_obj = null;
    AssetBundle _ab = null;
    Object _ab_obj = null;
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
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("bundle 1 loadfromfile"))
        {
            if(_ab == null)
                _ab = AssetBundle.LoadFromFile("AssetBundles/imgcontain12.prefab");
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
        GUILayout.EndHorizontal();
    }
}
