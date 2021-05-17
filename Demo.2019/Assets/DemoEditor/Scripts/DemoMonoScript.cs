using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 参考资料：
/// 1. Unity中常用的编辑器图形辅助线类 https://blog.csdn.net/Teddy_k/article/details/82117420
/// 
/// </summary>

public class DemoMonoScript : MonoBehaviour {

    public float radius = 1;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

#if UNITY_EDITOR
    private void OnDrawGizmos() // 在Scene视图实时显示
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position, Vector3.one);
    }

    private void OnDrawGizmosSelected() // 在Scene视图选中时显示
    {
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireCube(transform.position, 2 * Vector3.one);
    }
#endif
}
