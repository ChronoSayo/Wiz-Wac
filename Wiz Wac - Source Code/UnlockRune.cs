using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockRune : MonoBehaviour
{
    public Rune rune;
    
    private List<Transform> _runeIcons;

    public enum Rune
    {
        RemoteBomb, Magnesis, Stasis, Cryonis
    }

    void Start()
    {
        if (RuneHandler.UNLOCKED_RUNES[(int)rune + 1])
            Destroy(gameObject);
        else
        {
            _runeIcons = new List<Transform>();
            foreach(Transform t in transform)
            {
                if (!t.name.Contains("Cube"))
                    _runeIcons.Add(t);
            }
        }
    }

    void Update()
    {
        foreach (Transform t in _runeIcons)
        {
            t.Rotate(0, 1, 0);
        }
    }

    public void TurnOffIcon()
    {
        for(int i = 0; i < _runeIcons.Count; i++)
            _runeIcons[i].gameObject.SetActive(false);
    }
}
