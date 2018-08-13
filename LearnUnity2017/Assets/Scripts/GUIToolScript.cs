using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIToolScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    GameObject _last_test_mono_obj = null;
    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("AddTestMono"/*, GUILayout.Width(80)*/))
        {
            var go = new GameObject("TestMono");
            go.AddComponent<TestMonoScript>();
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
        GUILayout.EndHorizontal();
    }
}
