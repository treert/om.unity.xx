using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ABMgr
{
    public static readonly ABMgr singleton = new ABMgr();
    public AssetBundle LoadAB(string name, string path = null)
    {
        // 
        var ret = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault((ab) =>
        {
            return string.Compare(ab.name, name, true) == 0;
            //return ab.name == name;
        });
        if (ret != null)
        {
            return ret;
        }
#if UNITY_EDITOR
        ret = AssetBundle.LoadFromFile(path ?? $"AssetBundles/{name}");
#else
        
#endif
        return ret;
    }

    public void UnloadAllAB()
    {
        AssetBundle.UnloadAllAssetBundles(true);
    }

    public void LogSomeInfo()
    {
        //var bundles = Resources.FindObjectsOfTypeAll<AssetBundle>();// 这个接口不好，多返回了一个。
        var bundles = AssetBundle.GetAllLoadedAssetBundles();

        int i = 0;
        foreach (var ab in bundles)
        {
            Debug.Log($"Bundle {++i,3}: {ab.name}");
        }
        Debug.Log($"Bundle Count {i}");
    }
}
