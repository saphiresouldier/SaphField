using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : Singleton<SelectionManager>
{
    [SerializeField]
    private List<GameObject> currentSelectedObjects;

    // Start is called before the first frame update
    void Start()
    {
        GameObject test = GameObject.Find("Sphere");
        AddObjectToSelection(test);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AddObjectToSelection(GameObject selectedObject)
    {
        currentSelectedObjects.Add(selectedObject);

        var transformGizmo = RTG.RTGizmosEngine.Get.CreateObjectMoveGizmo();

        transformGizmo.SetTargetObject(selectedObject);
        transformGizmo.Gizmo.MoveGizmo.SetVertexSnapTargetObjects(new List<GameObject> { selectedObject });
        transformGizmo.SetTransformSpace(RTG.GizmoSpace.Local);

        Debug.Log("Added " + selectedObject.name + " to current selection!");
    }
}
