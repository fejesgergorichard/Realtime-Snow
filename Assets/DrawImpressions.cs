using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawImpressions : MonoBehaviour
{
    public int TrackResolution = 10;
    public Camera _camera;
    public Shader _drawShader;

    [Range(1, 500)]
    public float _bSize;

    [Range(0, 1)]
    public float _bStrength;

    private RenderTexture _splatmap;
    private Material _snowMaterial;
    private Material _drawMaterial;

    private RaycastHit _newHit;
    private RaycastHit _lastHit;

    // Use this for initialization
    void Start()
    {
        _drawMaterial = new Material(_drawShader);
        _drawMaterial.SetVector("_Color", Color.red);

        _snowMaterial = GetComponent<MeshRenderer>().material; // tesselation shader
        _splatmap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat);
        _snowMaterial.SetTexture("_Splatmap", _splatmap);

    }

    // Update is called once per frame
    void Update()
    {
        // listen for mouse press
        if (Input.GetKey(KeyCode.Mouse0))
        {
            // raycasting towards mesh
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out _newHit))
            {
                //DrawDot(new Vector4(_newHit.textureCoord.x, _newHit.textureCoord.y, 0, 0));
                DrawDots();
            }
        }

    }

    private void DrawDot(Vector4 coordinates)
    {
        _drawMaterial.SetVector("_Coordinates", coordinates);
        _drawMaterial.SetFloat("_Strength", _bStrength);
        _drawMaterial.SetFloat("_Size", _bSize);
        RenderTexture tmp = RenderTexture.GetTemporary(_splatmap.width, _splatmap.height, 0, RenderTextureFormat.ARGBFloat);
        Graphics.Blit(_splatmap, tmp);
        Graphics.Blit(tmp, _splatmap, _drawMaterial);
        RenderTexture.ReleaseTemporary(tmp);
    }

    private void DrawDots()
    {
        Vector4 lastCoords = new Vector4(_lastHit.textureCoord.x, _lastHit.textureCoord.y, 0, 0);
        Vector4 newCoords = new Vector4(_newHit.textureCoord.x, _newHit.textureCoord.y, 0, 0);
        Vector4 dist = (newCoords - lastCoords) / TrackResolution;

        for (int i = 0; i < TrackResolution; i++)
        {
            DrawDot(lastCoords + i * dist);
        }

        _lastHit = _newHit;
    }
}
