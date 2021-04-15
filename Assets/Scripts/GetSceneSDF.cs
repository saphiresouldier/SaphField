using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GetSceneSDF : Singleton<GetSceneSDF> {

    private SDF_Object[] _sdfs;
    private List<Transform> _sceneTransforms;

	// Use this for initialization
	public override void Awake () {
        base.Awake();

        _sceneTransforms = GetSDFsCurrentScene();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateScene()
    {
        _sceneTransforms = GetSDFsCurrentScene();
    }

    public SDF_Object[] GetSceneSDFs()
    {
        return _sdfs;
    }

    public List<Transform> GetSceneTransforms()
    {
        return _sceneTransforms;
    }

    private List<Transform> GetSDFsCurrentScene()
    {
        _sdfs = FindObjectsOfType<SDF_Object>(); // TODO: slow

        List<Transform> transforms = new List<Transform>();

        for(int i = 0; i < _sdfs.Length; i++)
        {
            Debug.Log("Found object with SDF_Object component: " + _sdfs[i].gameObject.name);

            transforms.Add(_sdfs[i].transform);
        }

        return transforms;
    }

    private Vector3 ComputeTriangleNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        Vector3 v3v1 = v3 - v1;
        Vector3 v2v1 = v2 - v1;
        return Vector3.Cross(v3v1, v2v1).normalized;
    }
}
