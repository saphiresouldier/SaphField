using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDF_Object : MonoBehaviour
{
    public OPTYPE opType = OPTYPE.UNION;
    [Range(0.0f, 1.0f)]
    public float SmoothRange = 0.5f;
    public Color color = Color.gray;

    //private void OnMouseUpAsButton()
    //{
    //    Debug.Log("Selected SDF: " + this.gameObject.name);
    //    SelectionManager.Instance.AddToCurrentSelection(this.gameObject, false);
    //}

    private void OnEnable()
    {
        GetSceneSDF.Instance.UpdateScene();
    }
}
