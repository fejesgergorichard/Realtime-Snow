using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelImpressions : MonoBehaviour {

    public Shader Shader;
    public GameObject[] Terrains;
    public Transform[] Wheels;
    [Range(0, 2)]
    public float WheelSize;

    [Range(0, 1)]
    public float WheelStrength;
    public int TrackResolution = 3;

    private Material _snow;
    private Material _drawMaterial;
    private RenderTexture _splatmap;
    private RaycastHit[] _newHits;
    private RaycastHit[] _lastHits;
    private int _mask;

    private Collider _currentPlane;
    // Use this for initialization
    void Start ()
    {
        _newHits = new RaycastHit[Wheels.Length];
        _lastHits = new RaycastHit[Wheels.Length];

        _mask = LayerMask.GetMask("Ground");

        _drawMaterial = new Material(Shader);

        for (int i = 0; i < Terrains.Length; i++)
        {
            _snow = Terrains[i].GetComponent<MeshRenderer>().material;
            _splatmap = new RenderTexture(2048, 2048, 0, RenderTextureFormat.ARGBFloat);
            _snow.SetTexture("_Splatmap", _splatmap);
        }
    }

    // Update is called once per frame
    void Update() {
        int speed = (int)GetComponent<Rigidbody>().velocity.magnitude;

        if (speed > 3)
            TrackResolution = 3;
        else if (speed < 3 && speed > 0)
            TrackResolution = speed;
        else if (speed < 1)
            TrackResolution = 1;

        for (int k = 0; k < Terrains.Length; k++)
        {
            for (int i = 0; i < Wheels.Length; i++)
            {
                // raycasting towards mesh
                if (Physics.Raycast(Wheels[i].position, -Vector3.up, out _newHits[i], 1f, _mask))
                {
                    DrawDots(i);
                }
            }
        }
    }

    private void DrawDot(Vector4 coordinates)
    {
        _drawMaterial.SetVector("_Coordinates", coordinates);
        _drawMaterial.SetFloat("_Strength", WheelStrength);
        _drawMaterial.SetFloat("_Size", WheelSize);
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
        Debug.Log(other.collider.GetType());
        if (other.collider.tag == "Ground" && other.collider != _currentPlane)
        {
            _currentPlane = other.collider;
            _snow = _currentPlane.GetComponent<MeshRenderer>().material;
            _splatmap = (RenderTexture)_snow.GetTexture("_Splatmap");
            _snow.SetTexture("_Splatmap", _splatmap);
        }
    }
}
