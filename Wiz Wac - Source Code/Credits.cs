using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{
    public float moveSpeed, rotSpeed;

    private int _currentPos, _stopText;
    private float _yaw, _pitch;
    private float _quitTime, _quitTick;
    private bool _showHint, _endCredits;
    private Transform _moveCameraHint;
    private Image _blackness;
    private RectTransform _creditsText;
    private List<Vector3> _positions;

    void Start ()
    {
        //Play credits song.
        if (Boss.DEFEATED)
        {
            AudioSource a = GetComponent<AudioSource>();
            a.playOnAwake = true;
            a.loop = false;
            a.volume = 1;
            a.Play();
        }

        _positions = new List<Vector3>();

        _creditsText = GameObject.Find("CreditsText").GetComponent<RectTransform>();
        _blackness = GameObject.Find("Blackness").GetComponent<Image>();

        foreach (Transform t in transform)
        {
            if (t.name == "Canvas")
                continue;
            _positions.Add(t.position);
        }

        _moveCameraHint = GameObject.Find("MoveCamera").transform;
        _moveCameraHint.gameObject.SetActive(false);

        _showHint = false;

        _currentPos = 0;

        //Y position of when "thank you" text is approximately in the middle of the screen.
        _stopText = 4750;

        _yaw = 60;
        _pitch = 30;

        _quitTick = 0;
        _quitTime = 2;
    }

    void Update()
    {
        ShowHint();
        Quit();
    }
    
    void FixedUpdate ()
    {
        CameraTravel();
        CreditsRoll();
    }

    private void CreditsRoll()
    {
        if (_creditsText.position.y < _stopText)
        {
            _creditsText.Translate(Vector3.up);
            _yaw += rotSpeed * Input.GetAxis("Mouse X");
            _pitch -= rotSpeed * Input.GetAxis("Mouse Y");
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0);
        }
        else
        {
            if (!_endCredits)
            {
                _endCredits = true;
                StartCoroutine(EndCredits());
            }
            _blackness.color += new Color(0, 0, 0, 0.01f);
        }
    }

    private void CameraTravel()
    {
        if (_currentPos < _positions.Count)
        {
            transform.position += (_positions[_currentPos] - transform.position).normalized * moveSpeed * Time.deltaTime;
            if (Vector3.Distance(transform.position, _positions[_currentPos]) < 1 && _currentPos <= _positions.Count)
                _currentPos++;
        }
    }

    private void ShowHint()
    {
        if((PlayerInput.GetAnyButtonDown || Input.anyKeyDown || 
            Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) && !_showHint)
        {
            _showHint = true;
            _moveCameraHint.gameObject.SetActive(true);
            StartCoroutine(DisableHint());
        }
    }

    private void Quit()
    {
        if (PlayerInput.Quit)
        {
            Tutorial.GetInstance.PlayTutorial("HoldQuit");
            _quitTick += Time.deltaTime;
            if (_quitTick >= _quitTime)
                Application.Quit();
        }
        else
            _quitTick = 0;
    }

    private IEnumerator EndCredits()
    {
        yield return new WaitForSeconds(5);

        SceneManager.LoadScene("start");
        Boss.DEFEATED = false;
    }

    private IEnumerator DisableHint()
    {
        yield return new WaitForSeconds(10);
        _moveCameraHint.gameObject.SetActive(false);
    }
}
