using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelImpressions : MonoBehaviour {

    public Shader _shader;
    public GameObject[] _terrains;
    public Transform[] _wheels;
    [Range(0, 2)]
    public float _bSize;

    [Range(0, 1)]
    public float _bStrength;
    public int TrackResolution = 3;

    private Material _snow;
    private Material _drawMaterial;
    private RenderTexture _splatmap;
    private RaycastHit[] _newHits = new RaycastHit[4];
    private RaycastHit[] _lastHits = new RaycastHit[4];
    private int _mask;

    private Collider _currentPlane;
    // Use this for initialization
    void Start () {
        _mask = LayerMask.GetMask("Ground");

        _drawMaterial = new Material(_shader);

        for (int i = 0; i < _terrains.Length; i++)
        {
            _snow = _terrains[i].GetComponent<MeshRenderer>().material;
            _splatmap = new RenderTexture(2048, 2048, 0, RenderTextureFormat.ARGBFloat);
            _snow.SetTexture("_Splatmap", _splatmap);
        }
    }

    // Update is called once per frame
    void Update() {
        int speed = (int)GetComponent<Rigidbody>().velocity.magnitude;
        TrackResolution = (speed < 3) ? speed : 3;

        for (int k = 0; k < _terrains.Length; k++)
        {
            for (int i = 0; i < _wheels.Length; i++)
            {
                // raycasting towards mesh
                if (Physics.Raycast(_wheels[i].position, -Vector3.up, out _newHits[i], 1f, _mask))
                {
                    DrawDots(i);
                }
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

    private void DrawDots(int n)
    {
        Vector4 lastCoords = new Vector4(_lastHits[n].textureCoord.x, _lastHits[n].textureCoord.y, 0, 0);
        Vector4 newCoords = new Vector4(_newHits[n].textureCoord.x, _newHits[n].textureCoord.y, 0, 0);
        Vector4 dist = (newCoords - lastCoords) / TrackResolution;

        for (int i = 0; i < TrackResolution; i++)
        {
            DrawDot(lastCoords + i * dist);
        }

        _lastHits[n] = _newHits[n];
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider != _currentPlane)
        {
            _currentPlane = other.collider;
            _snow = _currentPlane.GetComponent<MeshRenderer>().material;
            _splatmap = (RenderTexture)_snow.GetTexture("_Splatmap");
            _snow.SetTexture("_Splatmap", _splatmap);
        }
    }
}
