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

    public GUISkin m_skin;
    public GUIStyle m_style;

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

    string txtFieldString = "";
    string txtAreaString = "";

    private int selectionGridInt = 0;
    private string[] selectionStrings = { "Grid 1", "Grid 2", "Grid 3", "Grid 4" };

    private Vector2 scrollViewVector = Vector2.zero;
    private string innerText = "I am inside the ScrollView";
    Rect windowRect = new Rect(300, 20, 120, 50);
    private int selectedToolbar = 0;
    private string[] toolbarStrings = { "One", "Two" };
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), "label");
        if(GUI.Button(new Rect(10,40,100, 20), "btn"))
        {
            Debug.Log("click Button");
        }
        if (GUI.RepeatButton(new Rect(10, 70, 100, 20), "btn"))
        {
            Debug.Log("click RepeatButton");
        }
        txtFieldString = GUI.TextField(new Rect(10, 100, 100, 40), txtFieldString);
        txtAreaString = GUI.TextArea(new Rect(10, 150, 100, 40), txtAreaString);

        selectionGridInt = GUI.SelectionGrid(new Rect(10, 200, 200, 50), selectionGridInt, selectionStrings, 2);

        {
            // Begin the ScrollView
            scrollViewVector = GUI.BeginScrollView(new Rect(200, 10, 100, 100), scrollViewVector, new Rect(0, 0, 100, 100));

            // Put something inside the ScrollView
            innerText = GUI.TextArea(new Rect(0, 0, 400, 400), innerText);

            // End the ScrollView
            GUI.EndScrollView();
        }
        {
            windowRect = GUI.Window(0, windowRect, WindowFunction, "My Window");
        }
        {
            // Determine which button is active, whether it was clicked this frame or not
            selectedToolbar = GUI.Toolbar(new Rect(300, 100, Screen.width - 100, 30), selectedToolbar, toolbarStrings);

            // If the user clicked a new Toolbar button this frame, we'll process their input
            if (GUI.changed)
            {
                Debug.Log("The toolbar was clicked");

                if (0 == selectedToolbar)
                {
                    Debug.Log("First button was clicked");
                }
                else
                {
                    Debug.Log("Second button was clicked");
                }
            }
        }
    }

    void WindowFunction(int windowID)
    {
        txtAreaString = GUI.TextArea(new Rect(0, 0, 100, 40), txtAreaString);
        // Draw any Controls inside the window here
        // Make windows draggable
        GUI.DragWindow();
    }

    [ContextMenu("CallFunc_1")]
    void CallFunc_1()
    {
        print($"obj.Type = {(obj == null ? "null" : obj.GetType().FullName)}");
    }
}
