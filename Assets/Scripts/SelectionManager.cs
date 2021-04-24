using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum GizmoType
{
    MOVE = 0,
    ROTATE = 1,
    SCALE = 2,
    UNIVERSAL = 3,
    NONE = 4
}

public class SelectionManager : Singleton<SelectionManager>
{
    [SerializeField]
    private List<GameObject> currentSelectedObjects;

    //Gizmos
    private RTG.ObjectTransformGizmo[] gizmos;

    [SerializeField]
    private GizmoType currentGizmoType;

    internal GizmoType CurrentGizmoType { get => currentGizmoType; set => currentGizmoType = value; }


    // Start is called before the first frame update
    void Awake()
    {
        //GameObject test = GameObject.Find("Sphere");
        //AddObjectToSelection(test);
        gizmos = new RTG.ObjectTransformGizmo[(int)GizmoType.NONE];

        gizmos[(int)GizmoType.MOVE] = RTG.RTGizmosEngine.Get.CreateObjectMoveGizmo();
        gizmos[(int)GizmoType.ROTATE] = RTG.RTGizmosEngine.Get.CreateObjectRotationGizmo();
        gizmos[(int)GizmoType.SCALE] = RTG.RTGizmosEngine.Get.CreateObjectScaleGizmo();
        gizmos[(int)GizmoType.UNIVERSAL] = RTG.RTGizmosEngine.Get.CreateObjectUniversalGizmo();

        disableGizmos();
    }

    // Update is called once per frame
    void Update()
    {
        // TODO raycast selection? 


        disableGizmos();
        if(currentSelectedObjects.Count > 0)
        {
            UpdateGizmos();
        }
    }

    public void AddToCurrentSelection(GameObject selectedObject, bool expand = false)
    {
        if(expand == false) { ClearSelection(); }
        AddObjectToSelection(selectedObject);
    }

    private void disableGizmos()
    {
        foreach (RTG.ObjectTransformGizmo gizmo in gizmos)
        {
            gizmo.SetEnabled(false);
        }
    }

    private void UpdateGizmos()
    {
        gizmos[(int)currentGizmoType].SetEnabled(true);
    }

    private void ClearSelection()
    {
        currentSelectedObjects.Clear();
    }

    private void AddObjectToSelection(GameObject selectedObject)
    {
        currentSelectedObjects.Add(selectedObject);

        //TODO Update AABB of selected objects and place gizmo in center

        gizmos[(int)currentGizmoType].SetTargetObject(selectedObject);
        //gizmos[(int)currentGizmoType].Gizmo.MoveGizmo.SetVertexSnapTargetObjects(new List<GameObject> { selectedObject });
        gizmos[(int)currentGizmoType].SetTransformSpace(RTG.GizmoSpace.Local);

        Debug.Log("Added " + selectedObject.name + " to current selection!");
    }
}
