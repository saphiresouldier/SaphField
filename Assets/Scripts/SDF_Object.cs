using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDF_Object : MonoBehaviour
{
    public OPTYPE opType = OPTYPE.UNION;
    public SHAPE shape = SHAPE.SPHERE;
    [Range(0.0f, 1.0f)]
    public float SmoothRange = 0.5f;
    public Color color = Color.gray;

    private MeshRenderer meshRenderer;

    private void Awake()
    {
        if(meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

    }

    private void OnMouseEnter()
    {
        if (SelectionManager.Instance.highlightSelection && meshRenderer.enabled != true)
        {
            meshRenderer.enabled = true;
        }
    }

    private void OnMouseExit()
    {
        meshRenderer.enabled = false;
    }

    private void OnEnable()
    {
        GetSceneSDF.Instance.UpdateScene();
    }
}
