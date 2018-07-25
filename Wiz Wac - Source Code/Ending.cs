using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ending : MonoBehaviour
{
    public bool off = false;
    public Texture onMaterial, offMaterial;

    private Transform _endingCamera, _mainCamera;
    private Renderer _renderer;

    void Start ()
    {
        _mainCamera = GameObject.Find("FreeLookCameraRig").transform;

        _endingCamera = GameObject.FindGameObjectWithTag("EndingCamera").transform;
        _endingCamera.gameObject.SetActive(false);

        _renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (!off)
            InitiateEnding();
    }

    private void ChangeMaterial()
    {
        _renderer.material.mainTexture = off ? offMaterial : onMaterial;
    }

    private void InitiateEnding()
    {
        _mainCamera.gameObject.SetActive(false);
        _endingCamera.gameObject.SetActive(true);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.transform.name.Contains("Explosion"))
        {
            off = !off;
            ChangeMaterial();
            if (!off)
                InitiateEnding();
        }
    }
}
