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

    private Material _snow;
    private Material _draw;
    private RenderTexture _splatmap;
    private RaycastHit _hit;
    private int _mask;

    private Collider _currentPlane;
    // Use this for initialization
    void Start () {
        _mask = LayerMask.GetMask("Ground");

        _draw = new Material(_shader);

        for (int i = 0; i < _terrains.Length; i++)
        {
            _snow = _terrains[i].GetComponent<MeshRenderer>().material;
            _splatmap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat);
            _snow.SetTexture("_Splatmap", _splatmap);
        }
    }

    // Update is called once per frame
    void Update() {
        for (int k = 0; k < _terrains.Length; k++)
        {
            for (int i = 0; i < _wheels.Length; i++)
            {
                // raycasting towards mesh
                if (!Physics.Raycast(_wheels[i].position, -Vector3.up, out _hit, 1f, _mask))
                {
                    continue;
                }

                _draw.SetVector("_Coordinates", new Vector4(_hit.textureCoord.x, _hit.textureCoord.y, 0, 0));
                _draw.SetFloat("_Strength", _bStrength);
                _draw.SetFloat("_Size", _bSize);
                RenderTexture tmp = RenderTexture.GetTemporary(_splatmap.width, _splatmap.height, 0, RenderTextureFormat.ARGBFloat);
                Graphics.Blit(_splatmap, tmp);
                Graphics.Blit(tmp, _splatmap, _draw);
                RenderTexture.ReleaseTemporary(tmp);
            }
        }
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
