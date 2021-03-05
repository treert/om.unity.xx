using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.SceneManagement;
using Image = UnityEngine.UI.Image;

public class MyClass
{
    public UnityEngine.Object obj = null;
    public static UnityEngine.Object s_obj = null;
}
public class GUIToolScript : MonoBehaviour {
	// Use this for initialization
	void Start () {
        // 只在Editor里用用
        AssetBundle.UnloadAllAssetBundles(true);
        MyClass.s_obj = null;
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

    private HashSet<UnityEngine.Object> my_set = new HashSet<Object>();
    public UnityEngine.Object my_ref = null;
    public UnityEngine.Object my_ref2 = null;
    private MyClass my_obj = new MyClass();

    void AddRef(UnityEngine.Object obj)
    {
        // 所有的引用都会影响 Resources.UnloadUnusedAssets()
        //MyClass.s_obj = obj;
        //my_obj.obj = obj;
        //my_set.Add(obj);
        //my_ref = obj;
        //my_ref2 = obj;
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
        if (GUILayout.Button("destry object"))
        {
            //Object.DestroyImmediate(_ab_obj);
            var img = _GetTestImage();
            Object.DestroyImmediate(img);
        }
        if (GUILayout.Button("loadfromab2 1"))
        {
            if (_ab2)
            {
                var img = _GetTestImage();
                var sp = _ab2.LoadAsset<Sprite>("Assets/ABMgr/Res/Img/bg2.jpg");
                print($"sp instanceid {sp.GetInstanceID()} {sp.texture.GetInstanceID()}");
                if (img)
                {
                    AddRef(img.sprite);
                    img.sprite = sp;
                }
            }
            else
            {
                print("_ab2 is null");
            }
        }
        if (GUILayout.Button("loadfromab2 3"))
        {
            if (_ab2)
            {
                var img = _GetTestImage();
                var sp = _ab2.LoadAsset<Sprite>("Assets/ABMgr/Res/Img/bg3.jpg");
                print($"sp instanceid {sp.GetInstanceID()} {sp.texture.GetInstanceID()}");
                if (img)
                {
                    AddRef(img.sprite);
                    img.sprite = sp;
                }
            }
            else
            {
                print("_ab2 is null");
            }
        }
        if (GUILayout.Button("loadfromab2 3 1"))
        {
            if (_ab2)
            {
                var img = _GetTestImage();
                var sp = _ab2.LoadAsset<Sprite>("Assets/ABMgr/Res/Img/bg3.jpg");
                print($"sp instanceid {sp.GetInstanceID()} {sp.texture.GetInstanceID()}");
                if (img)
                {
                    AddRef(img.sprite);
                    img.sprite = null;// 
                    img.sprite = sp;
                }
            }
            else
            {
                print("_ab2 is null");
            }
        }
        if (GUILayout.Button("loadfromab2 prefab"))
        {
            if (_ab2)
            {
                var img = _GetTestImage();
                if (img)
                {
                    var go = _ab2.LoadAsset<GameObject>("Assets/ABMgr/Res/Prefab/ImgContain34.prefab");
                    var ab_img = go.transform.Find("Image3").GetComponent<Image>();
                    var sp = ab_img.sprite;
                    print($"sp instanceid {sp.GetInstanceID()} {sp.texture.GetInstanceID()}");
                    AddRef(img.sprite);
                    img.sprite = null;// 
                    img.sprite = sp;
                    //ab_img.sprite = null;// 修改就不对了
                }
            }
            else
            {
                print("_ab2 is null");
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        /**
         * 卸载逻辑的一些尝试总结：
         * 1. Resources.UnloadAsset 卸载AssetBundle加载的资源
         *    1. 在profiler的内存统计里是正确的。
         *    2. AB重新Load资源后，内存显示也确实有，然而UGUI不能及时刷新，图片还是黑的。
         *      - 先执行下`sprite = null` 就可以解决问题，UGUI赋值时需要这样才能感知到自己的内部资源发生了变化。
         *    3. https://issuetracker.unity3d.com/issues/serialized-asset-is-null-when-loaded-from-bundle-after-using-resources-dot-unloadasset
         *      - 这个官方回复说有问题，但是按那种说法，第二次加载应该内存里显示直接没有猜对。不懂了。
         *      - 版本 2018.2.3，大概已经没用了。
         *    4. 总结（Image.sprite.texture 为例，版本 2018.4.28）
         * 2. Resources.UnloadUnusedAssets 可以卸载AB里加载后没有引用的资源。
         * 3. 一些总结
         *    1. Resources.UnloadAsset 资源后，Sprite和Texture的bool判断状态还是true.
         *    2. Resources.UnloadAsset(img.sprite) 不会立即生效。（图片不变黑）
         *       - 后续执行 Resources.UnloadUnusedAssets() 后，卸载生效
         *       - Resources.UnloadAsset(img.sprite.texture) 会立即生效
         *    3. Resources.UnloadAsset 可以卸载AB里加载的资源，这个通过profiler可以看内存引用变化
         *       - demo 不生效的问题是spite 没有先`= null`，AB重新加载时，InstanceID没有变化，UGUI感知不到修改，UI没刷新。
         */
        if (GUILayout.Button("unload asset 1"))
        {
            // 有些坑，不能用于AB，
            UnityEngine.UI.Image img = _GetTestImage();
            var sp = img.sprite;
            print($"sp instanceid {sp.GetInstanceID()} {sp.texture.GetInstanceID()}");
            // 对于UGUI来说，不会立即生效，随后，调用UnloadUnusedAssets，图片就没了，显示变黑。
            Resources.UnloadAsset(img.sprite);
            print($"unload asset img.sprite={img.sprite == true} img.sprite.texture={img.sprite.texture == true} ");
        }
        if (GUILayout.Button("unload asset 2"))
        {
            // 有些坑，不能用于AB，
            UnityEngine.UI.Image img = _GetTestImage();
            var sp = img.sprite;
            print($"sp instanceid {sp.GetInstanceID()} {sp.texture.GetInstanceID()}");
            print($"before unload asset img.sprite={img.sprite == true} img.sprite.texture={img.sprite.texture == true}");
            //Resources.UnloadAsset(img.sprite);
            Resources.UnloadAsset(img.sprite.texture);// 立即生效。
            // 下面这个日志，会导致内存里还有tex，但是图片已经黑了
            // print($"after unload asset img.sprite={img.sprite==true} img.sprite.texture={img.sprite.texture == true} ");
        }
        if (GUILayout.Button("UnloadUnusedAssets"))
        {
            Resources.UnloadUnusedAssets();
        }
        if(GUILayout.Button("s_obj = null"))
        {
            MyClass.s_obj = null;
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("load all ab"))
        {
            _ab = ABMgr.singleton.LoadAB("imgcontain12.prefab");
            _ab2 = ABMgr.singleton.LoadAB("all_2.ab");
            _ab3 = ABMgr.singleton.LoadAB("all_3.ab");
            ABMgr.singleton.LoadAB("sub/prefab.ab");
            ABMgr.singleton.LoadAB("sub/img.ab");
        }
        if (GUILayout.Button("unload all ab"))
        {
            ABMgr.singleton.UnloadAllAB();
            Debug.Log($"_ab == null is {_ab == null}");
        }
        if (GUILayout.Button("bundle print name"))
        {
            ABMgr.singleton.LogSomeInfo();
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
