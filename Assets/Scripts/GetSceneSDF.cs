using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GetSceneSDF : Singleton<GetSceneSDF> {

    [SerializeField]
    private SDF_Object[] _sdfs;
    private bool updateScene = false;
    private bool updateSceneProperties = false;

	// Use this for initialization
	public override void Awake () {
        base.Awake();
	}
	
	// Update is called once per frame
	void Update () {
		if(updateScene)
        {
            GetSDFsCurrentScene();
            updateScene = false;
        }
	}

    public void UpdateScene()
    {
        updateScene = true;
    }

   public void UpdateSceneProperties()
    {
        // TODO
    }

    public SDF_Object[] GetSceneSDFs()
    {
        return _sdfs;
    }

    //public List<Transform> GetSceneTransforms()
    //{
    //    return _sceneTransforms;
    //}

    private void GetSDFsCurrentScene()
    {
        _sdfs = FindObjectsOfType<SDF_Object>(); // TODO: slow

        foreach(SDF_Object s in _sdfs)
        {
            Debug.Log("Found object with SDF_Object component: " + s.gameObject.name);
        }
    }
}
