using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchMagnet : MonoBehaviour
{
    public float contactRange;
    public Transform contactEffect;

    private List<Transform> _magnetizedObjects;
    private List<Transform> _contactEffects;

    void Start()
    {
        _contactEffects = new List<Transform>();
        _magnetizedObjects = new List<Transform>();
        foreach (Transform t in FindObjectsOfType<Transform>())
        {
            if (t.name.Contains("Magnet"))
            {
                _magnetizedObjects.Add(t);

                GameObject magnetEffect = Instantiate(contactEffect.gameObject, transform.position, Quaternion.identity);
                magnetEffect.GetComponent<Renderer>().enabled = false;
                _contactEffects.Add(magnetEffect.transform);
            }
        }
    }

    void Update()
    {
        for(int i = 0; i < _magnetizedObjects.Count; i++)
        {
            if(Vector3.Distance(transform.position, _magnetizedObjects[i].position) < contactRange)
                MagnetEffectBeam(_contactEffects[i], _magnetizedObjects[i]);
            else
            {
                if (_contactEffects[i].GetComponent<Renderer>().enabled)
                    TurnOnContactEffect(_contactEffects[i], false);
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.transform.name.Contains("Magnet"))
        {
            for (int i = 0; i < _contactEffects.Count; i++)
                TurnOnContactEffect(_contactEffects[i], false);
            Destroy(gameObject);
        }
    }

    private void MagnetEffectBeam(Transform contact, Transform hit)
    {
        if (!contact.GetComponent<Renderer>().enabled)
            TurnOnContactEffect(contact, true);
        contact.position = (hit.position + transform.position) / 2;
        Vector3 dir = (hit.position - transform.position).normalized;
        if (dir != Vector3.zero)
            contact.forward = dir;
        contact.localScale = new Vector3(0.5f, 0.5f, Vector3.Distance(transform.position, hit.position));
    }

    private void TurnOnContactEffect(Transform contact, bool turnOn)
    {
        contact.GetComponent<Renderer>().enabled = turnOn;
    }
}
