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
    public override void Awake()
    {
        base.Awake();
        gizmos = new RTG.ObjectTransformGizmo[(int)GizmoType.NONE];

        gizmos[(int)GizmoType.MOVE] = RTG.RTGizmosEngine.Get.CreateObjectMoveGizmo();
        gizmos[(int)GizmoType.ROTATE] = RTG.RTGizmosEngine.Get.CreateObjectRotationGizmo();
        gizmos[(int)GizmoType.SCALE] = RTG.RTGizmosEngine.Get.CreateObjectScaleGizmo();
        gizmos[(int)GizmoType.UNIVERSAL] = RTG.RTGizmosEngine.Get.CreateObjectUniversalGizmo();

        DisableGizmos();

        currentGizmoType = GizmoType.MOVE;
    }

    public void AddToCurrentSelection(GameObject selectedObject, bool expand = false)
    {
        if(gizmos[(int)currentGizmoType].Gizmo.IsHovered == false)
        {
            if (expand == false) { ClearSelection(); }
            AddObjectToSelection(selectedObject);
        }
    }

    public void ClearSelection()
    {
        if(gizmos[(int)currentGizmoType].Gizmo.IsHovered == false)
        {
            currentSelectedObjects.Clear();
            DisableGizmos();
        }
    }

    public void SetGizmoType(GizmoType newType)
    {
        if(newType != currentGizmoType)
        {
            DisableGizmo(gizmos[(int)currentGizmoType]);
            currentGizmoType = newType;
            if(currentSelectedObjects.Count > 0)
            {
                EnableGizmo(gizmos[(int)currentGizmoType], currentSelectedObjects[0]);
            }
        }
    }

    public GizmoType GetGizmoType()
    {
        return currentGizmoType;
    }

    private void DisableGizmos()
    {
        for(int i = 0; i < (int)GizmoType.NONE; i++)
        {
            DisableGizmo(gizmos[i]);
        }
    }

    private void UpdateGizmos()
    {
        if (currentSelectedObjects.Count > 0)
        {
            EnableGizmo(gizmos[(int)currentGizmoType], currentSelectedObjects[0]);
        }
    }

    private void AddObjectToSelection(GameObject selectedObject)
    {
        currentSelectedObjects.Add(selectedObject);

        //TODO Update AABB of selected objects and place gizmo in center

        EnableGizmo(gizmos[(int)currentGizmoType], selectedObject);
    }

    private void EnableGizmo(RTG.ObjectTransformGizmo gizmo, GameObject selection)
    {
        gizmo.SetEnabled(true);
        gizmo.Gizmo.SetEnabled(true);
        gizmo.SetTargetObject(selection); // TODO: multi object selection
        gizmo.SetTransformSpace(RTG.GizmoSpace.Local);

        // TODO
        //gizmos[(int)currentGizmoType].Gizmo.MoveGizmo.SetVertexSnapTargetObjects(new List<GameObject> { selection });
    }

    private void DisableGizmo(RTG.ObjectTransformGizmo gizmo)
    {
        gizmo.Gizmo.SetEnabled(false);
        gizmo.SetEnabled(false);
    }
}
