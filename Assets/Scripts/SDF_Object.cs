using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDF_Object : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
