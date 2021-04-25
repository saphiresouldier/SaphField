using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    private void OnMouseUpAsButton()
    {
        Debug.Log("Clicked Background");
        SelectionManager.Instance.ClearSelection();
    }
}
