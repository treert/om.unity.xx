using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LearnShadowVSMMono : MonoBehaviour
{
    public Camera m_camera;
    //public Transform m_light;
    public RenderTexture m_rt;
    public Shader m_shader;

    // Start is called before the first frame update
    void Start()
    {
        Shader.SetGlobalTexture("_CustomShadowMap_VSM", m_rt);

        m_camera.SetReplacementShader(m_shader, "RenderType");
    }

    // Update is called once per frame
    void Update()
    {
        //m_camera.transform.position = m_light.transform.position;
        //m_camera.transform.rotation = m_light.transform.rotation;
        //m_camera.transform.localScale = m_light.transform.localScale;
        Shader.SetGlobalMatrix("_Custom_World2Shadow_VSM", m_camera.projectionMatrix * m_camera.worldToCameraMatrix);
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Replace"))
        {
            m_camera.SetReplacementShader(m_shader, "RenderType");
        }
        if (GUILayout.Button("Reset"))
        {
            m_camera.ResetReplacementShader();

        }
        GUILayout.EndHorizontal();
    }

    // 对方差阴影贴图做 高斯模糊
}
