using System;
using System.Collections.Generic;
using UnityEngine;

public struct Sphere // 48
{
    public Vector3 position;
    public float radius;
    public RayMarchMaterial material;
};

public struct Triangle // 80
{
    public Vector3 v1, v2, v3;
    public Vector3 normal; //flat shading will do for now
    public RayMarchMaterial material;
};

// TODO: evaluate if directly using SDF_Object is possible here
public struct SDF // 56
{
    public Vector3 pos, rot, scale, color;
    public float smoothRange;
    public int opType;
}

public struct RayMarchMaterial // 32
{
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public float emission;
};

[Flags]
public enum UPDATEFLAGS
{
    NONE = 0,
    RESTART_SAMPLING = 1,
    REBUILD_SCENE = 2
    // TODO UPDATE_SCENE
}

public enum OPTYPE
{
    UNION = 0,
    SUBTRACTION = 1,
    INTERSECTION = 2,
    SMOOTHUNION = 3,
    SMOOTHSUBTRACTION = 4,
    SMOOTHINTERSECTION = 5
}

public class RaytracingController : MonoBehaviour {

    public ComputeShader RayTraceShader;
    public Camera MainCamera;
    public Camera GizmoCamera;
    public Texture SkyboxTex;
    public Light DirectionalLight;

    // TODO: remove
    private Vector2 SphereRadius = new Vector2(3.0f, 8.0f);
    private uint SpheresMax = 100;
    private float SpherePlacementRadius = 100.0f;

    public int RandomSeed = 0;

    [SerializeField]
    private float _skyboxMultiplicator = 1.0f;
    private RenderTexture _targetTex;
    private RenderTexture _convergingTex;
    private uint _currentSample = 0;
    private Material _addMaterial;

    private ComputeBuffer _sphereBuffer;
    private ComputeBuffer _triangleBuffer;
    private ComputeBuffer _sdfBuffer;
    private int oldSDFCount = 0;

    private UPDATEFLAGS updateFlags = 0;

    public float SkyboxMultiplicator
    {
        get { return _skyboxMultiplicator; }
        set {
            _skyboxMultiplicator = value;
            updateFlags |= UPDATEFLAGS.RESTART_SAMPLING;
        }
    }

    private void Awake()
    {
        if (MainCamera == null) {
            MainCamera = Camera.main;
        }

        UnityEngine.Random.InitState(RandomSeed); 
    }

    private void OnEnable()
    {
        _currentSample = 0;
        //SetupSphereScene();
        //SetupTriangleScene();
        SetupSDFScene();
    }

    private void OnDisable()
    {
        //_sphereBuffer.Release();
        //_triangleBuffer.Release();
        _sdfBuffer.Release();
    }

    private void Update()
    {
        DetectTransformChanged(MainCamera.transform);
        DetectTransformChanged(DirectionalLight.transform);
        DetectTransformChanged(GizmoCamera.transform);
        foreach (SDF_Object s in GetSceneSDF.Instance.GetSceneSDFs())
        {
            DetectTransformChanged(s.transform, true); //TODO: optimize
        }

        if(updateFlags.HasFlag(UPDATEFLAGS.REBUILD_SCENE))
        {
            SetupSDFScene();
            RestartSampling();
            updateFlags &= (~UPDATEFLAGS.REBUILD_SCENE);
            updateFlags |= UPDATEFLAGS.RESTART_SAMPLING;
        }
        
        if (updateFlags.HasFlag(UPDATEFLAGS.RESTART_SAMPLING))
        {
            RestartSampling();
            updateFlags &= (~UPDATEFLAGS.RESTART_SAMPLING);
        }
    }

    public void DebugUpdate()
    {
        SetupSDFScene();
        RestartSampling();
    }

    private void DetectTransformChanged(Transform t, bool rebuildScene = false)
    {
        if (t.hasChanged)
        {
            if (rebuildScene)
            {
                updateFlags |= UPDATEFLAGS.REBUILD_SCENE;
            }
            else
            {
                updateFlags |= UPDATEFLAGS.RESTART_SAMPLING;
            }
            t.hasChanged = false;

            RestartSampling();
        }
    }

    private void SetupSphereScene()
    {
        List<Sphere> spheres = new List<Sphere>();
        // Add a number of random spheres
        for (int i = 0; i < SpheresMax; i++)
        {
            Sphere sphere = new Sphere();
            sphere.material = new RayMarchMaterial();

            // Radius and radius
            sphere.radius = SphereRadius.x + UnityEngine.Random.value * (SphereRadius.y - SphereRadius.x);
            Vector2 randomPos = UnityEngine.Random.insideUnitCircle * SpherePlacementRadius;
            sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);

            // Reject spheres that are intersecting others
            foreach (Sphere other in spheres)
            {
                float minDist = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist)
                {
                    goto SkipSphere;
                }
                    
            }
            // Albedo and specular color
            Color color = UnityEngine.Random.ColorHSV();
            bool metal = UnityEngine.Random.value < 0.5f;
            bool emissive = UnityEngine.Random.value < 0.2f; //TODO: optimization potential, skip metal computation of emissive
            sphere.material.albedo = metal ? Vector3.zero : new Vector3(color.r, color.g, color.b);
            sphere.material.specular = metal ? new Vector3(color.r, color.g, color.b) : Vector3.one * 0.04f;
            sphere.material.smoothness = UnityEngine.Random.value;
            sphere.material.emission = emissive ? 1.0f : 0.0f;

            // Add the sphere to the list
            spheres.Add(sphere);

        SkipSphere:
            continue;
        }
        // Assign to compute buffer, 48 is byte size of sphere struct in memory
        _sphereBuffer = new ComputeBuffer(spheres.Count, 48);
        _sphereBuffer.SetData(spheres);
    }

    private void SetupSDFScene()
    {
        List<SDF> sdfs = GetSceneSDFs();
        Debug.Log("Got transforms from SDF_Objects, transforms contains " + sdfs.Count + " sdfs!");

        // Assign to compute buffer, 56 is byte size of sdf struct in memory
        if(_sdfBuffer == null || (sdfs.Count != oldSDFCount)) {
            _sdfBuffer = new ComputeBuffer(sdfs.Count, 56);
            oldSDFCount = sdfs.Count;
        }
        _sdfBuffer.SetData(sdfs);
    }

    private List<SDF> GetSceneSDFs()
    {
        List<SDF> sdfs = new List<SDF>();

        var sdf_objects  = GetSceneSDF.Instance.GetSceneSDFs();

        for(int i = 0; i < sdf_objects.Length; i++)
        {
            SDF sdf = new SDF();
            sdf.pos = sdf_objects[i].transform.position;
            sdf.rot = sdf_objects[i].transform.rotation.eulerAngles;
            sdf.scale = sdf_objects[i].transform.localScale;
            sdf.color = new Vector3(sdf_objects[i].color.r, sdf_objects[i].color.g, sdf_objects[i].color.b);
            sdf.smoothRange = sdf_objects[i].SmoothRange;
            sdf.opType = (int)sdf_objects[i].opType;

            sdfs.Add(sdf);
        }

        return sdfs;
    }

    private void SetupTriangleScene()
    {
        List<Triangle> tris = GetSceneTriangles(false);
        Debug.Log("Got triangles from MeshFilters, _triangleBuffer contains " + tris.Count + " triangles!");

        // Assign to compute buffer, 80 is byte size of triangle struct in memory
        _triangleBuffer = new ComputeBuffer(tris.Count, 80);
        _triangleBuffer.SetData(tris);
    }

    private List<Triangle> GetSceneTriangles(bool generated = false)
    {
        List<Triangle> triangles = new List<Triangle>();

        if(generated)
        {
            // Add a number of random spheres
            for (int i = 0; i < SpheresMax; i++)
            {
                Triangle tri = new Triangle();
                tri.material = new RayMarchMaterial();
                float radius = SphereRadius.x + UnityEngine.Random.value * (SphereRadius.y - SphereRadius.x);
                Vector2 randomPos = UnityEngine.Random.insideUnitCircle * SpherePlacementRadius;
                Vector3 position = new Vector3(randomPos.x, radius, randomPos.y);
                tri.v1 = UnityEngine.Random.onUnitSphere * radius + position;
                tri.v2 = UnityEngine.Random.onUnitSphere * radius + position;
                tri.v3 = UnityEngine.Random.onUnitSphere * radius + position;
                tri.normal = ComputeTriangleNormal(tri.v1, tri.v2, tri.v3);

                // Albedo and specular color
                Color color = UnityEngine.Random.ColorHSV();
                bool metal = UnityEngine.Random.value < 0.0f; //TODO
                tri.material.albedo = metal ? Vector3.zero : new Vector3(color.r, color.g, color.b);
                tri.material.specular = metal ? new Vector3(color.r, color.g, color.b) : Vector3.one * 0.04f;

                // Add the sphere to the list
                triangles.Add(tri);
            }
        }
        else // Get actual Unity scene triangles
        {
            triangles = GetSceneMeshes.Instance.GetSceneTriangles();
        }

        return triangles;
    }

    private Vector3 ComputeTriangleNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        Vector3 v3v1 = v3 - v1;
        Vector3 v2v1 = v2 - v1;
        return Vector3.Cross(v2v1, v3v1).normalized;
    }

    //Main render function
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }

    private void SetShaderParameters()
    {
        RayTraceShader.SetMatrix("_CameraToWorld", MainCamera.cameraToWorldMatrix);
        RayTraceShader.SetMatrix("_CameraInverseProjection", MainCamera.projectionMatrix.inverse);
        RayTraceShader.SetTexture(0, "_SkyboxTex", SkyboxTex);
        RayTraceShader.SetFloat("_SkyboxTexFactor", _skyboxMultiplicator);
        RayTraceShader.SetFloat("_Seed", UnityEngine.Random.value);
        RayTraceShader.SetVector("_PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
        Vector3 l = DirectionalLight.transform.forward;
        RayTraceShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));
        //RayTraceShader.SetBuffer(0, "_Spheres", _sphereBuffer);
        //RayTraceShader.SetBuffer(0, "_Triangles", _triangleBuffer);
        RayTraceShader.SetBuffer(0, "_SDFs", _sdfBuffer);
    }

    private void Render(RenderTexture dest)
    {
        //Do we have a render target?
        InitRenderTexture();

        //set target, dispatch computeshader
        RayTraceShader.SetTexture(0, "Result", _targetTex);
        int threadGroupAmountX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupAmountY = Mathf.CeilToInt(Screen.height / 8.0f);
        RayTraceShader.Dispatch(0, threadGroupAmountX, threadGroupAmountY, 1);

        // show resulting texture
        // Blit the result texture to the screen
        if (_addMaterial == null)
            _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        _addMaterial.SetFloat("_Sample", _currentSample);
        Graphics.Blit(_targetTex, _convergingTex, _addMaterial);
        Graphics.Blit(_convergingTex, dest);
        _currentSample++;
    }

    private void InitRenderTexture()
    {
        int UIAreaRight = UIManager.Instance.UIAreaRight;
        if (_targetTex == null || _targetTex.width != Screen.width - UIAreaRight || _targetTex.height != Screen.height)
        {
            //Restart with sample 0
            _currentSample = 0;

            //release old render texture
            if (_targetTex != null) _targetTex.Release();

            //Create render texture for raytracing
            _targetTex = new RenderTexture(Screen.width - UIAreaRight, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _targetTex.enableRandomWrite = true;
            _targetTex.Create();
        }

        if (_convergingTex == null || _convergingTex.width != Screen.width - UIAreaRight || _convergingTex.height != Screen.height)
        {
            //Restart with sample 0
            _currentSample = 0;

            //release old render texture
            if (_convergingTex != null) _convergingTex.Release();

            //Create render texture for raytracing
            _convergingTex = new RenderTexture(Screen.width - UIAreaRight, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _convergingTex.enableRandomWrite = true;
            _convergingTex.Create();
        }
    }

    private void RestartSampling()
    {
        //Debug.Log("RestartSampling()");
        _currentSample = 0;
    }
}
