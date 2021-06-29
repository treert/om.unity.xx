using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestMono : MonoBehaviour
{
    // Start is called before the first frame update
    public List<string> m_list_str;
    public string[] m_arr_str;
    public Dictionary<string, string> m_dict_str;
    void Start()
    {
        Debug.Log($"TestMono Start");
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"TestMono Update");
    }

    public HideFlags m_SetHideFlag;
    [ContextMenu("SetFlag4ThisCom")]
    void SetFlag4ThisCom()
    {
        hideFlags = m_SetHideFlag;
    }

    [ContextMenu("SetFlag4GameObject")]
    void SetFlag4GameObject()
    {
        this.gameObject.hideFlags = m_SetHideFlag;
    }
}
