using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GizmoType
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
        gizmos = new RTG.ObjectTransformGizmo[(int)GizmoType.NONE];

        gizmos[(int)GizmoType.MOVE] = RTG.RTGizmosEngine.Get.CreateObjectMoveGizmo();
        gizmos[(int)GizmoType.ROTATE] = RTG.RTGizmosEngine.Get.CreateObjectRotationGizmo();
        gizmos[(int)GizmoType.SCALE] = RTG.RTGizmosEngine.Get.CreateObjectScaleGizmo();
        gizmos[(int)GizmoType.UNIVERSAL] = RTG.RTGizmosEngine.Get.CreateObjectUniversalGizmo();

        disableGizmos();

        currentGizmoType = GizmoType.MOVE;
    }

    public void AddToCurrentSelection(GameObject selectedObject, bool expand = false)
    {
        if(expand == false) { ClearSelection(); }
        AddObjectToSelection(selectedObject);
    }

    public void ClearSelection()
    {
        if(gizmos[(int)currentGizmoType].Gizmo.IsHovered == false)
        {
            currentSelectedObjects.Clear();
            disableGizmos();
        }
    }

    public void SetGizmoType(GizmoType newType)
    {
        if(newType != currentGizmoType)
        {
            gizmos[(int)currentGizmoType].Gizmo.SetEnabled(false);
            gizmos[(int)currentGizmoType].SetEnabled(false);
            currentGizmoType = newType;
            if(currentSelectedObjects.Count > 0)
            {
                gizmos[(int)currentGizmoType].SetEnabled(true);
                gizmos[(int)currentGizmoType].Gizmo.SetEnabled(true);
                gizmos[(int)currentGizmoType].SetTargetObject(currentSelectedObjects[0]);
                gizmos[(int)currentGizmoType].SetTransformSpace(RTG.GizmoSpace.Local);
            }
        }
    }

    public GizmoType GetGizmoType()
    {
        return currentGizmoType;
    }

    private void disableGizmos()
    {
        for(int i = 0; i < (int)GizmoType.NONE; i++)
        {
            gizmos[i].Gizmo.SetEnabled(false);
            gizmos[i].SetEnabled(false);
        }
    }

    private void UpdateGizmos()
    {
        if (currentSelectedObjects.Count > 0)
        {
            gizmos[(int)currentGizmoType].SetEnabled(true);
            gizmos[(int)currentGizmoType].Gizmo.SetEnabled(true);
            gizmos[(int)currentGizmoType].SetTargetObject(currentSelectedObjects[0]); // TODO: multi object selection
            gizmos[(int)currentGizmoType].SetTransformSpace(RTG.GizmoSpace.Local);
        }
    }

    private void AddObjectToSelection(GameObject selectedObject)
    {
        currentSelectedObjects.Add(selectedObject);

        //TODO Update AABB of selected objects and place gizmo in center

        gizmos[(int)currentGizmoType].SetEnabled(true);
        gizmos[(int)currentGizmoType].Gizmo.SetEnabled(true);
        gizmos[(int)currentGizmoType].SetTargetObject(selectedObject);
        //gizmos[(int)currentGizmoType].Gizmo.MoveGizmo.SetVertexSnapTargetObjects(new List<GameObject> { selectedObject });
        gizmos[(int)currentGizmoType].SetTransformSpace(RTG.GizmoSpace.Local);
    }
}
