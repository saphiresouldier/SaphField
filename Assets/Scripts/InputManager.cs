using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane))
            {
            
                Debug.Log("You selected the " + hit.transform.name); // DEBUG
                SelectionManager.Instance.AddToCurrentSelection(hit.transform.gameObject, false);
            
            }
            else
            {
                Debug.Log("You selected the background");
                SelectionManager.Instance.ClearSelection();

            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            SelectionManager.Instance.SetGizmoType(GizmoType.MOVE);
        }
        else if(Input.GetKeyDown(KeyCode.E))
        {
            SelectionManager.Instance.SetGizmoType(GizmoType.ROTATE);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            SelectionManager.Instance.SetGizmoType(GizmoType.SCALE);
        }
        //else if (Input.GetKeyDown(KeyCode.T))
        //{
        //    SelectionManager.Instance.SetGizmoType(GizmoType.UNIVERSAL);
        //}
    }
}
