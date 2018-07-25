using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchLeader : MonoBehaviour
{
    public bool off = false;
    public Texture onMaterial, offMaterial;
    public Transform contactEffect;

    private Renderer _renderer;
    private bool _turnOffContactEffect;
    private List<Transform> _switchAffectObjects;
    private List<Transform> _contactEffects;

    void Start()
    {
        _switchAffectObjects = new List<Transform>();
        _contactEffects = new List<Transform>();
        _renderer = GetComponent<Renderer>();

        foreach (Transform t in transform)
        {
            Transform contact = t;
            if(t.name == "MovePlatform")
            {
                foreach(Transform tChild in t.GetComponentsInChildren<Transform>())
                {
                    if (tChild.name == "Platform")
                        contact = tChild;
                }
            }
            _switchAffectObjects.Add(contact);

            GameObject switchEffect = Instantiate(contactEffect.gameObject, transform.position, Quaternion.identity);
            _contactEffects.Add(switchEffect.transform);
        }

        if(_contactEffects.Count == 0)
        {
            _switchAffectObjects.Add(GameObject.Find("SwitchAffect Special").transform);
            GameObject switchEffect = Instantiate(contactEffect.gameObject, transform.position, Quaternion.identity);
            _contactEffects.Add(switchEffect.transform);
        }

        _turnOffContactEffect = false;

        ChangeMaterial();
    }

    void Update()
    {
        if (!_turnOffContactEffect)
            ShowBeams();
    }

    private void ShowBeams()
    {
        for (int i = 0; i < _switchAffectObjects.Count; i++)
            SwitchEffectBeam(_contactEffects[i], _switchAffectObjects[i]);
    }

    private void SwitchEffectBeam(Transform contact, Transform hit)
    {
        contact.position = (hit.position + transform.position) / 2;
        Vector3 dir = (hit.position - transform.position).normalized;
        if (dir != Vector3.zero)
            contact.forward = dir;
        contact.localScale = new Vector3(0.5f, 0.5f, Vector3.Distance(transform.position, hit.position));
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.transform.name.Contains("Explosion"))
        {
            off = !off;
            ChangeMaterial();
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.transform.name.Contains("Magnet"))
        {
            off = !off;
            ChangeMaterial();
        }
    }

    private void ChangeMaterial()
    {
        _renderer.material.mainTexture = off ? offMaterial : onMaterial;
    }

    public void TurnOffContactEffect()
    {
        _turnOffContactEffect = true;
        _switchAffectObjects.Clear();

        for (int i = 0; i < _contactEffects.Count; i++)
            Destroy(_contactEffects[i].gameObject);
        _contactEffects.Clear();
    }
}
