using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

//[AddComponentMenu("Test/Mod Name")]
public class MonoDemo : MonoBehaviour
{
    public UnityEngine.UI.Image image;
    public Sprite sprite;
    public SpriteAtlas atlas;
    public Object obj;

    private void Reset()
    {
        // 没有用，obj会先被重置掉
        //print($"obj.Type = {(obj==null? "null": obj.GetType().Name)}");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("CallFunc_1")]
    void CallFunc_1()
    {
        print($"obj.Type = {(obj == null ? "null" : obj.GetType().FullName)}");
    }
}
