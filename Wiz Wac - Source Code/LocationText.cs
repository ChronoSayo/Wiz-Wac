using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocationText : MonoBehaviour
{
    private GameObject _greatPlateauTrigger;
    private Text _locationText;
    private Coroutine _timer;
    private string _currentText;
    private string _greatPlateau, _kingdomOfHyoctrule;
    private bool _showingHyoctrule;
    private State _state;
    private List<Transform> _hyoctruleTriggers;

    private static bool _SHOWED_PLATEAU;
    private static bool _SHOWED_KINGDOM;

    private enum State
    {
        None, Show, End
    }

    void Start()
    {
        _locationText = GameObject.Find("Location").GetComponent<Text>();
        _currentText = "";
        _greatPlateau = "SEMI-GREAT PLATEAU";
        _kingdomOfHyoctrule = "KINGDOM OF HYOCTRULE";

        if (_SHOWED_PLATEAU)
            GameObject.Find(_greatPlateau).SetActive(false);

        SetLocationsTriggers();

        _showingHyoctrule = false;
    }

    private void SetLocationsTriggers()
    {
        _hyoctruleTriggers = new List<Transform>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Location"))
        {
            if (go.name == _kingdomOfHyoctrule)
            {
                _hyoctruleTriggers.Add(go.transform);
                if (_SHOWED_KINGDOM)
                    go.SetActive(false);
            }
        }
    }

    void Update()
    {
        FadeState();
    }

    private void FadeState()
    {
        switch (_state)
        {
            case State.None:
                break;
            case State.Show:
                Fade(true);
                break;
            case State.End:
                Fade(false);
                break;
        }
    }

    private void ShowLocation(string locName)
    {
        if (_showingHyoctrule)
        {
            _currentText = locName;
            return;
        }

        if (_currentText != "")
            StopCoroutine(_timer);
        _currentText = locName;
        PlayMessage();
    }

    private void PlayMessage()
    {
        _state = State.Show;
        _locationText.text = _currentText;

        Color c = _locationText.color;
        _locationText.color = new Color(c.r, c.g, c.b, 0);

        _timer = StartCoroutine(EndMessage());
    }

    private IEnumerator EndMessage()
    {
        yield return new WaitForSeconds(10);
        _state = State.End;
    }

    public void ShowPlateauName()
    {
        ShowLocation(_greatPlateau);
    }

    private void Fade(bool show)
    {
        float speed = 0.0025f;
        if (!show)
            speed *= -1;

        _locationText.color += new Color(0, 0, 0, speed);

        Color c = _locationText.color;
        c.a = Mathf.Clamp(c.a, 0, 1);
        _locationText.color = c;

        if (_locationText.color.a <= 0)
        {
            if (_showingHyoctrule)
            {
                _showingHyoctrule = false;
                PlayMessage();
            }
            else
            {
                _currentText = "";
                _state = State.None;
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Location" && col.name != _currentText)
        {
            if (col.name == _greatPlateau)
            {
                _SHOWED_PLATEAU = true;
                col.gameObject.SetActive(false);
            }
            else if (col.name == _kingdomOfHyoctrule)
            {
                if (RuneHandler.RUNES_UNLOCKED)
                {
                    ShowLocation("KINGDOM OF\nHYOCTRULE\n");
                    _SHOWED_KINGDOM = true;
                    foreach (Transform t in _hyoctruleTriggers)
                        t.gameObject.SetActive(false);
                    _showingHyoctrule = true;
                }
            }
            else
                ShowLocation(col.name);
        }
    }

    void OnTriggerStay(Collider col)
    {
        if(_showingHyoctrule && col.tag == "Location")
        {
            _currentText = col.name;
        }
    }
}
