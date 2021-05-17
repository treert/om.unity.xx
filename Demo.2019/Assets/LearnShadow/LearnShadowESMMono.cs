using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LearnShadowESMMono : MonoBehaviour
{
    public Camera m_camera;
    public RenderTexture m_rt;
    public Shader m_shader;

    public float m_esm_c = 80;

    // Start is called before the first frame update
    void Start()
    {
        Shader.SetGlobalTexture("_CustomShadowMap_ESM", m_rt);
        m_camera.SetReplacementShader(m_shader, "RenderType");
    }

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalMatrix("_Custom_World2Shadow_ESM", m_camera.projectionMatrix * m_camera.worldToCameraMatrix);
        Shader.SetGlobalFloat("_esm_c", m_esm_c);
    }

    private void OnGUI()
    {
        GUILayout.Space(50);
        GUILayout.TextField("ESM");
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
