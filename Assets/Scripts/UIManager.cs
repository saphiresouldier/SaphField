using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    //public RaytracingController raytracingController;
    public int UIAreaRight = 150;
    public Camera MainCamera;
    public Camera UICamera;
    public Camera GizmoCamera;

    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
        Cursor.visible = true;

        SetCameras();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetCameras()
    {
        //rescale cam rect for UI
        Rect camRect = MainCamera.rect;
        float relativeYSize = (Screen.width - (float)UIAreaRight) / Screen.width;
        camRect.xMax = relativeYSize;
        //Debug.Log("Screenwidth: " + Screen.width + " minus UIAreRight: " + UIAreaRight + " equals " + camRect.xMax);
        MainCamera.rect = camRect;
        GizmoCamera.rect = camRect;
        camRect = UICamera.rect;
        camRect.xMin = relativeYSize;
        camRect.xMax = 1.0f;
        //Debug.Log("Screenwidth: " + Screen.width + " minus UIAreRight: " + UIAreaRight + " equals " + camRect.xMax);
        UICamera.rect = camRect;
    }
}
