using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMonoScript : MonoBehaviour {

    private void Awake()
    {
        Debug.Log("TestMono Awake");
    }

    private void OnEnable()
    {
        Debug.Log("TestMono OnEnable");
    }
    // Use this for initialization
    void Start () {
        Debug.Log("TestMono Start");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
