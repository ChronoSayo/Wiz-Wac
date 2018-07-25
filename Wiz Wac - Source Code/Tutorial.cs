using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Hint system. I don't know why I called it tutorial.
public class Tutorial : MonoBehaviour
{
    private float _timer;
    private bool _pause;

    private static float _TICK;
    private static Transform _CURRENT_TUTORIAL;
    private static string _CURRENT_NAME;
    private static Dictionary<string, bool> _TUTORIALS;
    private static List<string> _LINING_TUTORIALS;

    public static Tutorial GetInstance;

    void Awake()
    {
        GetInstance = this;
    }

    void Start ()
    {
        if (_TUTORIALS == null)
        {
            _TUTORIALS = new Dictionary<string, bool>();
            foreach (Transform t in transform)
            {
                _TUTORIALS.Add(t.name, false);
                t.gameObject.SetActive(false);
            }
            _CURRENT_TUTORIAL = null;
        }
        else
        {
            string current = "";
            foreach (string s in _TUTORIALS.Keys)
            {
                GetTransform(s).gameObject.SetActive(false);
                if (s == _CURRENT_NAME)
                    current = s;
            }

            if(current != "")
                Begin(current);
        }

        ReAddTutorials();

        _timer = 13;

        _LINING_TUTORIALS = new List<string>();
    }

    private void ReAddTutorials()
    {
        _TUTORIALS["HoldQuit"] = false;

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "magnet journey" || GameplayHandler.IS_BOSS)
        {
            string s = "MagnetFly";
            if (_TUTORIALS[s])
                _TUTORIALS[s] = false;
        }
        if (sceneName == "cryonis" || sceneName == "magnesis" || sceneName == "remote bomb" || sceneName == "stasis")
        {
            string s = "DisableRuneHelp";
            if (_TUTORIALS[s])
                _TUTORIALS[s] = false;
        }
        if (sceneName == "spider")
        {
            string s = "UpsideDownClimb";
            if (_TUTORIALS[s])
                _TUTORIALS[s] = false;
        }
        if (GameplayHandler.IS_BOSS)
        {
            string s1 = "SuperThrow";
            string s2 = "MagnetBomb";
            string s3 = "StasisAffect";
            if (_TUTORIALS[s1])
                _TUTORIALS[s1] = false;
            if (_TUTORIALS[s2])
                _TUTORIALS[s2] = false;
            if (_TUTORIALS[s3])
                _TUTORIALS[s3] = false;
        }
    }

    void Update()
    {
        if (_pause)
            return;

        if(_CURRENT_TUTORIAL)
        {
            _TICK += Time.deltaTime;
            if(_TICK >= _timer)
                Disable();
        }
    }

    public void PlayTutorial(string newName)
    {
        //Debug
        if (_TUTORIALS == null)
            return;

        if (!_TUTORIALS[newName])
        {
            if (!Save(newName))
                Begin(newName);
        }
    }

    private void Begin(string newName)
    {
        _CURRENT_TUTORIAL = GetTransform(newName);
        _CURRENT_TUTORIAL.gameObject.SetActive(true);
        _TUTORIALS[_CURRENT_TUTORIAL.name] = true;
        _CURRENT_NAME = _CURRENT_TUTORIAL.name;
    }

    public void Pause()
    {
        _pause = true;
        if(_CURRENT_TUTORIAL)
            _CURRENT_TUTORIAL.gameObject.SetActive(false);
    }

    public void Resume()
    {
        _pause = false;
        if (_CURRENT_TUTORIAL)
            _CURRENT_TUTORIAL.gameObject.SetActive(true);
    }

    private bool Save(string next)
    {
        if (_CURRENT_TUTORIAL && !UpNext(next))
            _LINING_TUTORIALS.Add(next);

        return _CURRENT_TUTORIAL;
    }

    private bool UpNext(string newName)
    {
        return _LINING_TUTORIALS.Contains(newName);
    }

    private void Disable()
    {
        _CURRENT_TUTORIAL.gameObject.SetActive(false);
        _CURRENT_TUTORIAL = null;
        _CURRENT_NAME = "";
        _TICK = 0;

        if (_LINING_TUTORIALS.Count > 0)
        {
            PlayTutorial(_LINING_TUTORIALS[0]);
            _LINING_TUTORIALS.RemoveAt(0);
        }
    }

    private Transform GetTransform(string name)
    {
        Transform key = null;
        foreach (Transform t in transform)
        {
            if(t.name == name)
            {
                key = t;
                break;
            }
        }
        return key;
    }
}
