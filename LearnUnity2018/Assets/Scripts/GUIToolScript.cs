using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
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
    public AssetBundle _ab = null;
    public AssetBundle _ab2 = null;
    public AssetBundle _ab3 = null;
    public UnityEngine.Object _ab_obj = null;

    public GameObject _editor_load_go = null;

    UnityEngine.UI.Image _GetTestImage()
    {
        var root = GameObject.Find("Canvas");
        var img_tra = root.transform.Find("ImgContain12/Image1");
        var img = img_tra.GetComponent<UnityEngine.UI.Image>();
        return img;
    }

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
        if (GUILayout.Button("unload asset"))
        {
            Resources.UnloadAsset(_GetTestImage().sprite.texture);// 有些坑，不能用于AB，
        }
        if (GUILayout.Button("destry object"))
        {
            Object.DestroyImmediate(_ab_obj);
            var img = _GetTestImage();
            Object.DestroyImmediate(img);
        }
        if (GUILayout.Button("loadfromab2"))
        {
            if (_ab2)
            {
                var img = _GetTestImage();
                var sp = _ab2.LoadAsset<Sprite>("Assets/ABMgr/Res/Img/bg1.jpg");
                print($"_ab_obj exsit {(_ab_obj ? true : false) }");
                _ab_obj = _ab2.LoadAsset<GameObject>("ImgContain12");
                _ab_obj = GameObject.Instantiate(_ab_obj);
                print($"sp instanceid {sp.GetInstanceID()}");
                if(img) img.sprite = sp;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("load all ab"))
        {
            if(_ab == null)
                _ab = AssetBundle.LoadFromFile("AssetBundles/imgcontain12.prefab");
            if (_ab2 == null)
                _ab2 = AssetBundle.LoadFromFile("AssetBundles/all_2.ab");
            _ab3 = AssetBundle.LoadFromFile("AssetBundles/all_3.ab");
        }
        if (GUILayout.Button("bundle 2 load, conflict"))
        {
            _ab2 = AssetBundle.LoadFromFile("AssetBundles/all_2.ab");
            _ab3 = AssetBundle.LoadFromFile("AssetBundles/all_3.ab");
            // 冲突没有发生，实践证明，AssetBundle加载冲突是以当初打ab时的名字来比较的。
        }
        if (GUILayout.Button("unload all ab"))
        {
            AssetBundle.UnloadAllAssetBundles(true);
            // Unity的null判断重载过，不写这些，_ab == null 一样返回true
            //_ab = null;
            //_ab2 = null;
            //_ab3 = null;
        }
        if (GUILayout.Button("bundle print name"))
        {
            //var bundles = Resources.FindObjectsOfTypeAll<AssetBundle>();// 这个接口不好，多返回了一个。
            var bundles = AssetBundle.GetAllLoadedAssetBundles();

            int i = 0;
            foreach(var ab in bundles)
            {
                Debug.Log($"Bundle {++i,3}: {ab.name}");
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
