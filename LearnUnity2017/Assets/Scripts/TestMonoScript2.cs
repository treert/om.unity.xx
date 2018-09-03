using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMonoScript2 : MonoBehaviour {

    private void Awake()
    {
        Debug.Log("TestMono2 Awake");
    }

    private void OnEnable()
    {
        Debug.Log("TestMono2 OnEnable");
    }
    // Use this for initialization
    void Start()
    {
        Debug.Log("TestMono2 Start");
    }

    // Update is called once per frame
    void Update () {
		
	}
}
