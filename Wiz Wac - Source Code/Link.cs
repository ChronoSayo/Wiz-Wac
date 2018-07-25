using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Cameras;
using System.IO;

//Player.
public class Link : MonoBehaviour
{
    public Transform cameraRig, whiteFlash;
    public float maxSpeedDrive, maxSpeedClimb, maxTurboSpeedAdditive;
    public float acceleration;
    public float jumpForceMultiplied;

    private Stasis _stasisScript;
    private Fuel _fuelScript;
    private FreeLookCam _freeLookCameraScript;
    private ProtectCameraFromWallClip _otherFreeLookCamerScript;
    private RawImage _flash;
    private GameObject _minimap;
    private GameObject _helpPad, _helpKeyboard, _helpRunes;
    private Transform _cinematicTrigger;
    private ParticleSystem _thruster, _exhaust;
    private Rigidbody _rigidbody;
    private Vector3 _oldWallNormal, _oldForward, _startPosition, _startRotation;
    private Vector3 _wheelClimbSize, _wheelDefaultSize;
    private float _speed, _delayTick, _airSpinX, _airSpinZ;
    private float _mouseSpeed, _cameraTurnSpeed, _camDistance;
    private float _quitTime, _quitTick, _climbIdleTime, _climbIdleTick;
    private int _currentStartPosition;
    private bool _climbed, _canClimb, _enterCinematic, _drifting, _jetting, _turbo;
    private bool _teleport;
    private List<Transform> _wheels;
    private CarState _carState;
    private DriveState _driveState;

    private static bool SHOW_CONTROLS = true;
    private static bool SHOW_RUNES = false;
    private static int PHOTO_COUNTER = 1;
    //Overworld
    private static List<Vector3> START_POSITIONS;

    private enum DriveState
    {
        None, Accelerate, Decelerate, Park
    }

    private enum CarState
    {
        None, Driving, Climbing, Airborne, StandBy, Dead
    }

    void Start ()
    {
        _stasisScript = GetComponent<Stasis>();
        _fuelScript = GetComponent<Fuel>();
        _rigidbody = GetComponent<Rigidbody>();

        _wheels = new List<Transform>();

        foreach(Transform t in transform)
        {
            if (t.name == "Thruster")
                _thruster = t.GetComponent<ParticleSystem>();
            else if (t.name == "Exhaust")
                _exhaust = t.GetComponent<ParticleSystem>();
            if (t.name.Contains("Left"))
                _wheels.Add(t);
            if (t.name.Contains("Right"))
                _wheels.Add(t);
        }
        Vector3 wheelSize = _wheels[0].localScale;
        _wheelDefaultSize = wheelSize;
        _wheelClimbSize = new Vector3(wheelSize.x, 0.7f, wheelSize.z);

        TurnOffFireEffects();

        _carState = CarState.Driving;
        _driveState = DriveState.Park;

        if (START_POSITIONS == null)
        {
            START_POSITIONS = new List<Vector3>();
            START_POSITIONS.Add(transform.position);
        }
        _startPosition = transform.position;
        _startRotation = transform.rotation.eulerAngles;

        _currentStartPosition = -1;

        _teleport = false;

        _oldWallNormal = Vector3.zero;

        cameraRig.position = transform.position;
        _freeLookCameraScript = cameraRig.GetComponent<FreeLookCam>();
        _mouseSpeed = _freeLookCameraScript.m_MoveSpeed;
        _cameraTurnSpeed = _freeLookCameraScript.m_TurnSpeed;

        _flash = GameObject.Find("Flash").GetComponent<RawImage>();

        _otherFreeLookCamerScript = cameraRig.GetComponent<ProtectCameraFromWallClip>();
        _camDistance = _otherFreeLookCamerScript.closestDistance;

        _canClimb = true;
        _drifting = false;
        _jetting = false;
        _turbo = false;

        _oldForward = Vector3.zero;

        _helpPad = GameObject.Find("HelpControls");
        _helpKeyboard = GameObject.Find("HelpKeyboard");
        _helpRunes = GameObject.Find("HelpRunes");

        if (!SHOW_CONTROLS)
        {
            _helpPad.SetActive(false);
            _helpKeyboard.SetActive(false);
        }
        else
        {
            if(StartMenu.USE_KEYBOARD)
                _helpPad.SetActive(false);
            else
                _helpKeyboard.SetActive(false);
        }
        _helpRunes.SetActive(false);

        _quitTick = 0;
        _quitTime = 2;
        _climbIdleTick = 0;
        _climbIdleTime = 3;
    }

    void Update()
    {
        if (_stasisScript.ChargedStasis)
        {
            cameraRig.GetComponent<FreeLookCam>().m_TurnSpeed = _cameraTurnSpeed;
            return;
        }

        if (!IsDead)
            DetectDead();

        switch (_carState)
        {
            case CarState.None:
                break;
            case CarState.Driving:
                Quit();
                PressingRestart();
                HandleTutorialScreens();
                if (_canClimb)
                    DetectClimb();
                RotateTowardGround();
                CarControls();
                HandleTurbo();
                CameraControl();
                _fuelScript.Spend(Mathf.Abs(_speed));
                break;
            case CarState.Airborne:
                Quit();
                PressingRestart();
                HandleTutorialScreens();
                RotateTowardGround();
                break;
            case CarState.Climbing:
                Quit();
                PressingRestart();
                HandleTutorialScreens();
                if (_carState != CarState.Climbing)
                    return;
                DetectCorner();
                CarControls();
                RotateToClimbSurface();
                _fuelScript.Spend(_speed);
                _fuelScript.AddSpending(50);
                if (_fuelScript.NoFuel)
                    RunLatchOff();
                else
                {
                    if (_speed == 0)
                    {
                        _climbIdleTick += Time.deltaTime;
                        if (_climbIdleTick >= _climbIdleTime)
                            Tutorial.GetInstance.PlayTutorial("ClimbIdle");
                    }
                    else
                        _climbIdleTick = 0;
                }
                break;
            case CarState.StandBy:
                break;
            case CarState.Dead:
                Quit();
                break;
        }
    }
    
    void FixedUpdate()
    {
        if (_stasisScript.ChargedStasis)
            return;

        switch (_carState)
        {
            case CarState.None:
                break;
            case CarState.Driving:
                Accelerate();
                Decelerate();
                MoveCar();
                Steer();
                DriftRotate();
                Decelerate();
                Turbo();
                break;
            case CarState.Airborne:
                Decelerate();
                MoveInAir();
                if (PlayerInput.DriftHold && _carState != CarState.Driving)
                    SpinControl();
                else
                    Steer();
                Spin();
                GravityDown();
                AirControls();
                break;
            case CarState.Climbing:
                Accelerate();
                Decelerate();
                MoveCar();
                Steer();
                break;
            case CarState.StandBy:
                break;
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

    private void PressingRestart()
    {
        if (PlayerInput.Restart)
        {
            if (GameplayHandler.IS_OVERWORLD && ControlCar && !IsDead)
                HandleTeleport();
            else if (!GameplayHandler.IS_OVERWORLD)
            {
                GameplayHandler.LOAD_TEXT.gameObject.SetActive(true);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    public void Teleport()
    {
        _currentStartPosition++;
        if (_currentStartPosition >= START_POSITIONS.Count)
            _currentStartPosition = 0;
        transform.position = START_POSITIONS[_currentStartPosition];
        _teleport = false;
        transform.rotation = Quaternion.Euler(_startRotation);
    }

    private void HandleTeleport()
    {
        EnterCinematic = true;
        _teleport = true;
        _speed = 0;
        Tutorial.GetInstance.PlayTutorial("Teleport");
    }

    private void HandleTutorialScreens()
    {
        if (SHOW_CONTROLS)
        {
            bool showControls = PlayerInput.HelpPad != 0 || PlayerInput.HelpKeyboard || PlayerInput.HelpRunesKB;
            if (showControls)
                SHOW_CONTROLS = false;
        }
        else if(SHOW_RUNES)
        {
            bool showRunes = PlayerInput.HelpPad != 0 || PlayerInput.HelpKeyboard || PlayerInput.HelpRunesKB;
            if (showRunes)
                SHOW_RUNES = false;
        }
        else
        {
            if (PlayerInput.HelpPad < 0)
            {
                TurnOffTutorialScreen();
                _helpPad.SetActive(true);
            }
            else if (PlayerInput.HelpKeyboard)
            {
                TurnOffTutorialScreen();
                _helpKeyboard.SetActive(true);
            }
            else if(PlayerInput.HelpRunesKB || PlayerInput.HelpPad > 0)
            {
                TurnOffTutorialScreen();
                _helpRunes.SetActive(true);
            }
            else
                TurnOffTutorialScreen();

        }
    }

    private void TurnOffTutorialScreen()
    {
        _helpKeyboard.SetActive(false);
        _helpPad.SetActive(false);
        _helpRunes.SetActive(false);
    }

    private void CameraControl()
    {
        _freeLookCameraScript.DisableRotation = PlayerInput.CameraResetHold;

        if (PlayerInput.FPSHold)
        {
            _otherFreeLookCamerScript.closestDistance = 0;
            if (RuneHandler.RUNES_UNLOCKED && PlayerInput.UseRune)
                Selfie();
        }
        else
            _otherFreeLookCamerScript.closestDistance = _camDistance;
    }

    private void Selfie()
    {
        Tutorial.GetInstance.PlayTutorial("Selfie");

        string album = "Wiz Wac Album";
        string folder = album;
#if !UNITY_EDITOR
                folder = Application.dataPath + "/../" + album;
#endif
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        string filePath = folder + "/Wiz Wac Photo " + PHOTO_COUNTER + ".png";


        _minimap = GameObject.Find("Minimap");
        if (_minimap)
            _minimap.SetActive(false);
        CamerPhotoGUI(false);

        Application.CaptureScreenshot(filePath);
        PHOTO_COUNTER++;

        StartCoroutine(TurnOnGUI());
    }

    private IEnumerator TurnOnGUI()
    {
        yield return new WaitForEndOfFrame();
        if(_minimap)
            _minimap.SetActive(true);

        CamerPhotoGUI(true);

        CameraFlash(true);

        StartCoroutine(TurnOffFlash());
    }

    private void CamerPhotoGUI(bool show)
    {
        //Free Cam Rig => Pivot => Main Camera => Canvas
        Transform canvas = _freeLookCameraScript.transform.GetChild(0).GetChild(0).GetChild(0);
        canvas.gameObject.SetActive(show);
    }

    private IEnumerator TurnOffFlash()
    {
        yield return new WaitForSeconds(0.08f);

        CameraFlash(false);
    }

    private void CameraFlash(bool on)
    {
        _flash.color = new Color(1, 1, 1, on ? 1 : 0);
    }

    private void DetectDead()
    {
        bool runesUnlocked = RuneHandler.RUNES_UNLOCKED;
        bool isOverworld = SceneManager.GetActiveScene().name == GameplayHandler.OVERWORLDLEVEL;
        float defaultDeathY = -30;
        float deathY =
            isOverworld ?
                runesUnlocked ?
                    defaultDeathY : 430 :
                        defaultDeathY;
        if (transform.position.y < deathY)
            HandleDeath();
    }

    private void HandleDeath()
    {
        _carState = CarState.Dead;
        StartCoroutine(RestartPosition());
        StartCoroutine(FakeRagDoll());

        cameraRig.GetComponent<FreeLookCam>().m_MoveSpeed = 0;

        if (!RuneHandler.RUNES_UNLOCKED && GameplayHandler.IS_OVERWORLD)
        {
            ShowMessage.GetInstance.PlayMessage(
                "You are still inadequate to leave the plateau. Complete the four Shrines to prove your worth.",
                15);
        }
    }

    private IEnumerator FakeRagDoll()
    {
        yield return new WaitForFixedUpdate();
        _rigidbody.AddForce(Vector3.down * 10, ForceMode.VelocityChange);
    }

    private IEnumerator RestartPosition()
    {
        yield return new WaitForSeconds(2);

        if (GameplayHandler.IS_OVERWORLD)
            transform.position = START_POSITIONS[0];
        else
            transform.position = _startPosition;
        transform.rotation = Quaternion.Euler(_startRotation);
        _speed = 0;
        _oldForward = Vector3.zero;
        cameraRig.GetComponent<FreeLookCam>().m_MoveSpeed = _mouseSpeed;

        TurnOnDriving();
    }

    private IEnumerator ResetMotion()
    {
        yield return new WaitForFixedUpdate();
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _speed = 0;
    }

    private void GravityDown()
    {
        if (!Input.GetKey(KeyCode.Space))
            _rigidbody.AddForce(Physics.gravity * 1.5f, ForceMode.Acceleration);
        else
            _rigidbody.AddForce(Physics.gravity / 2, ForceMode.Acceleration);
    }

    private IEnumerator TurnOffClimb(float sec)
    {
        _canClimb = false;
        yield return new WaitForSeconds(sec);
        _canClimb = true;
    }

    private void DetectGround(RaycastHit hit, bool downward = false)
    {
        Vector3 up = transform.up;
        transform.position = GetSurfacePointOffset(hit);
        transform.rotation = Quaternion.LookRotation(hit.normal);
        transform.forward = -_oldWallNormal;
        
        if (downward)
            transform.Rotate(0, 180, 0);

        //Check if slope is not angled toward car.
        if (Vector3.Cross(hit.normal, _oldWallNormal).x == 0)
        {
            float angledGround = Vector3.Angle(hit.normal, Vector3.up);

            //To check if the angle should be negative or positive.
            Vector3 cross = Vector3.Cross(hit.normal, transform.forward);

            angledGround *= cross.y > 0 ? 1 : -1;
            transform.Rotate(0, 0, angledGround);
        }

        if (_carState != CarState.Driving)
            TurnOnDriving();
    }

    private void RotateToClimbSurface()
    {
        RaycastHit underHit, frontHit;
        bool underCar = Physics.Raycast(transform.position, -transform.up, out underHit, 2);
        Vector3 dir = _driveState == DriveState.Accelerate ? transform.forward :
            _driveState == DriveState.Decelerate ? -transform.forward :
            Vector3.zero;
        bool drivingDirHit = Physics.Raycast(transform.position, dir, out frontHit, 1);
        if (underCar && DriveAngle(underHit.normal))
        {
            _speed = 0;
            TurnOnDriving();
        }
        if ((underCar || drivingDirHit) && _carState == CarState.Climbing)
        {
            if ((drivingDirHit && frontHit.transform.tag == "Climbable") || (underCar && underHit.transform.tag == "Climbable"))
                CalculateWallSurface(drivingDirHit ? frontHit : underHit);
            else if ((drivingDirHit && frontHit.transform.tag == "Terrain") || (underCar && underHit.transform.tag == "Terrain"))
                CalculateTerrainSurface(drivingDirHit ? frontHit : underHit);
        }
    }

    private void TurnOnAirborne()
    {
        _rigidbody.useGravity = true;
        _rigidbody.isKinematic = false;
        _carState = CarState.Airborne;
        _climbed = false;
        _oldForward = transform.forward;
        _airSpinX = 0;
        _airSpinZ = 0;
        _drifting = false;
        _exhaust.Stop();
        ClimbingWheels();
    }

    private void TurnOnClimbing(RaycastHit hit)
    {
        if(!_fuelScript.NoFuel)
        {
            _speed = 0;
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
            _carState = CarState.Climbing;
            _climbed = true;
            _drifting = false;
            _turbo = false;
            _thruster.Stop();
            _exhaust.Stop();
            _jetting = false;
            ClimbingWheels();
        }
    }

    private void TurnOnDriving()
    {
        _rigidbody.useGravity = true;
        _rigidbody.isKinematic = false;
        _carState = CarState.Driving;
        _oldWallNormal = Vector3.zero;
        _climbed = false;
        cameraRig.GetComponent<FreeLookCam>().m_TurnSpeed = _cameraTurnSpeed;
        _thruster.Stop();
        _jetting = false;
        ClimbingWheels();
    }

    private void ClimbingWheels()
    {
        if (_carState == CarState.Climbing)
        {
            foreach (Transform t in _wheels)
                t.localScale = _wheelClimbSize;
        }
        else
        {
            foreach (Transform t in _wheels)
                t.localScale = _wheelDefaultSize;
        }
    }

    private bool Grounded()
    {
        Vector3 downDir = _carState == CarState.Driving ? Vector3.down : -transform.up;
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, downDir, out hit, 1))
        {
            if (_carState != CarState.Airborne)
                TurnOnAirborne();
        }
        else
        {
            if (hit.transform.tag == "Climbable" || hit.transform.tag == "Terrain")
            {
                if (_climbed)
                {
                    if (_carState != CarState.Climbing)
                        TurnOnClimbing(hit);
                }
                else
                {
                    if (_carState != CarState.Driving)
                        TurnOnDriving();
                }
            }
            else if(hit.transform.tag == "Untagged")
            {
                if (_carState != CarState.Driving)
                    TurnOnDriving();
            }
        }

        return _carState != CarState.Airborne;
    }

    private void DetectClimb()
    {
        if (Vector3.Angle(transform.forward, Vector3.up) < 35)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -transform.up, out hit, 3))
            {
                bool climbWall = hit.transform.tag == "Climbable" && Vector3.Angle(hit.normal, Vector3.up) > 90;
                bool climbTerrain = hit.transform.tag == "Terrain" && !PlayerInput.TurboHold;
                bool boostUp = hit.transform.tag == "Terrain" && PlayerInput.TurboHold;

                if (climbWall || climbTerrain)
                {
                    ClimbSurface(hit);
                }
                else if (boostUp)
                    Tutorial.GetInstance.PlayTutorial("BoostUpTerrain");
            }
        }
    }

    private void ClimbSurface(RaycastHit hit)
    {
        if (hit.transform.tag == "Climbable")
            Tutorial.GetInstance.PlayTutorial("Climb");
        if (hit.transform.tag == "Terrain")
            Tutorial.GetInstance.PlayTutorial("ClimbTerrain");
        TurnOnClimbing(hit);
        CalculateWallSurface(hit);
    }

    private void DetectCorner()
    {
        if (CheckEmptyRayFromDriveState() || CheckForNormalChange())
            return;
        
        RaycastHit hitCorner, hitFront;
        bool checkCorner = CheckCornerRayFromDriveState(out hitCorner);
        if (checkCorner && !CheckWallInFrontOrBackFromDriveState(out hitFront, 4))
        {
            if (DriveAngle(hitCorner.normal))
                DetectGround(hitCorner);
            else
                CalculateWallSurface(hitCorner);
        }
        else if (!checkCorner && CheckWallInFrontOrBackFromDriveState(out hitFront, 2))
            DetectGround(hitFront, true);
    }

    private bool DriveAngle(Vector3 hitNormal)
    {
        return Vector3.Angle(hitNormal, Vector3.down) > 130;
    }

    private bool CheckEmptyRayFromDriveState(float forwardOffset = 1.3f)
    {
        float rayDistance = 3;

        return (_driveState == DriveState.Accelerate && Physics.Raycast(transform.position + (transform.forward * forwardOffset),
            transform.forward - transform.up, rayDistance)) ||
            (_driveState == DriveState.Decelerate && Physics.Raycast(transform.position + (-transform.forward * forwardOffset),
            -transform.forward - transform.up, rayDistance));
    }

    private bool CheckCornerRayFromDriveState(out RaycastHit hit)
    {
        hit = new RaycastHit();
        float forwardOffset = 0.5f;
        float rayDistance = 3;
        return (_driveState == DriveState.Accelerate && Physics.Raycast(transform.position + (transform.forward * forwardOffset) - transform.up, 
            -transform.forward - transform.up, out hit, rayDistance)) ||
            (_driveState == DriveState.Decelerate && Physics.Raycast(transform.position + (-transform.forward * forwardOffset) - transform.up,
            transform.forward - transform.up, out hit, rayDistance));
    }

    private bool CheckForNormalChange()
    {
        float forwardOffset = 0.5f;
        float rayDistance = 3;
        Vector3 dir = _driveState == DriveState.Accelerate ? transform.forward :
            _driveState == DriveState.Decelerate ? -transform.forward : 
            Vector3.zero;

        if (dir == Vector3.zero)
            return false;

        return Physics.Raycast(transform.position + (dir * forwardOffset), -transform.up, rayDistance);
    }

    private bool CheckWallInFrontOrBackFromDriveState(out RaycastHit hit, float rayDistance)
    {
        hit = new RaycastHit();
        bool wallFound = (_driveState == DriveState.Accelerate && Physics.Raycast(transform.position, transform.forward, out hit, rayDistance)) ||
            (_driveState == DriveState.Decelerate && Physics.Raycast(transform.position, -transform.forward, out hit, rayDistance));
        return wallFound;
    }

    private void CalculateWallSurface(RaycastHit hit)
    {
        if (_oldWallNormal == hit.normal)
            return;

        transform.position = GetSurfacePointOffset(hit);

        if (_oldWallNormal == Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(-hit.normal);
            transform.rotation = Quaternion.Euler(new Vector3(-Vector3.Angle(hit.normal, Vector3.up), transform.eulerAngles.y,
                transform.eulerAngles.z));
        }
        else if (hit.normal == Vector3.down)
        {
            Tutorial.GetInstance.PlayTutorial("UpsideDownClimb");
            BumpOnWall();
        }
        else
        {
            float angleX = transform.eulerAngles.x;

            bool rightPointDown = Vector3.Angle(transform.right, Vector3.down) > 89;
            transform.rotation = Quaternion.LookRotation(hit.normal);

            float angleZ = Vector3.Angle(hit.normal, transform.right);
            angleZ = rightPointDown ? angleZ : -angleZ;

            float angleY = rightPointDown ? 90 : -90;

            transform.Rotate(angleX, angleY, angleZ);
        }

        _oldWallNormal = hit.normal;

    }
    
    private void CalculateTerrainSurface(RaycastHit hit)
    {
        if (_oldWallNormal == hit.normal)
            return;

        transform.position = GetSurfacePointOffset(hit);

        if (_oldWallNormal == Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(-hit.normal);
            transform.rotation = Quaternion.Euler(new Vector3(-Vector3.Angle(hit.normal, Vector3.up), transform.eulerAngles.y,
                transform.eulerAngles.z));
        }
        else
        {
            float angleX = transform.eulerAngles.x;

            bool rightPointDown = Vector3.Angle(transform.right, Vector3.down) > 89;
            transform.rotation = Quaternion.LookRotation(hit.normal);

            float angleZ = Vector3.Angle(hit.normal, transform.right);
            angleZ = rightPointDown ? angleZ : -angleZ;

            float angleY = rightPointDown ? 90 : -90;

            transform.Rotate(angleX, angleY, angleZ);
        }

        _oldWallNormal = hit.normal;
    }

    private Vector3 GetSurfacePointOffset(RaycastHit hit)
    {
        return hit.point + (hit.normal * 0.5f);
    }

    private void AirControls()
    {
        if (Physics.Raycast(transform.position, -transform.up, 3) && Vector3.Angle(transform.forward, Vector3.up) < 30)
            WallPush();
        else
            StartCoroutine(Jet());

        AirBrakes();
    }

    private void AirBrakes()
    {
        if(PlayerInput.HandBrake)
        {
            _airSpinX = 0;
            _airSpinZ = 0;
        }
    }

    private void SpinControl()
    {
        float multiply = 14;
        _airSpinX += PlayerInput.Vertical * Time.deltaTime * multiply;
        _airSpinZ -= PlayerInput.Steer * Time.deltaTime * multiply;

        if (_airSpinX != 0 || _airSpinZ != 0)
            Tutorial.GetInstance.PlayTutorial("Tricks");

        LimitSpin(ref _airSpinX);
        LimitSpin(ref _airSpinZ);
    }

    private void Spin()
    {
        transform.Rotate(_airSpinX, 0, _airSpinZ);

        DecreaseSpin(ref _airSpinX);
        DecreaseSpin(ref _airSpinZ);
        LimitSpin(ref _airSpinX);
        LimitSpin(ref _airSpinZ);
    }

    private void DecreaseSpin(ref float spinValue)
    {
        float decreaseSpin = 0.01f;
        if (spinValue > 0)
            spinValue -= decreaseSpin;
        if (spinValue < 0)
            spinValue += decreaseSpin;
    }

    private void LimitSpin(ref float spinValue)
    {
        float maxSpin = 5;
        if (spinValue > maxSpin)
            spinValue = maxSpin;
        if (spinValue < -maxSpin)
            spinValue = -maxSpin;
    }

    private void MoveInAir()
    {
        if (_stasisScript.GetLockAirMovement)
            return;

        if (_jetting)
        {
            if (_oldForward.normalized != transform.forward.normalized)
            {
                Vector3 steerDir = Vector3.zero;
                if (Vector3.Angle(_oldForward.normalized, transform.right.normalized) < 88)
                    steerDir = -transform.right;
                else if (Vector3.Angle(_oldForward.normalized, transform.right.normalized) > 92)
                    steerDir = transform.right;
                _oldForward += transform.forward.normalized + steerDir;
            }
        }

        transform.position += _oldForward.normalized * _speed * Time.deltaTime;
    }

    private void CarControls()
    {
        HandBrake();
        MinMaxSpeedLimit();
        if (_carState == CarState.Driving)
        {
            Drift();
            Jump();
        }
        if (_carState == CarState.Climbing)
            LatchOff();
        DriveStateHandler();
    }

    private void Drift()
    {
        if (PlayerInput.DriftDown)
        {
            _drifting = true;
            _oldForward = transform.forward;
        }
        else if(PlayerInput.DriftUp)
            _drifting = false;
    }

    private void DriftRotate()
    {
        if (PlayerInput.DriftHold)
        {
            transform.Rotate(0, PlayerInput.Steer * (Time.deltaTime / 200), 0);
        }
    }

    private void RotateTowardGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.up, out hit, 2))
        {
            _rigidbody.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
            if (_carState != CarState.Driving)
                TurnOnDriving();
        }
    }

    private void Jump()
    {
        if (PlayerInput.JumpOnce && !_fuelScript.NoFuel)
        {
            TurnOnAirborne();
            StartCoroutine(Jumping(transform.up));
            _fuelScript.Spend(GetJumpForce);
        }
    }

    private IEnumerator Jumping(Vector3 direction)
    {
        yield return new WaitForFixedUpdate();
        _rigidbody.AddForce(direction * GetJumpForce, ForceMode.Impulse);
    }

    private Vector3 RampJumpDirection()
    {
        float scaledSpeed = _speed / 10;
        Vector3 v = Vector3.zero;
        v = _driveState == DriveState.Accelerate ? transform.forward : -transform.forward;
        return v * scaledSpeed;
    }

    private bool CheckRamp()
    {   
        RaycastHit hit;
        bool rampUnder = false;
        
        if (!CheckEmptyRayFromDriveState(2))
        {
            if (_speed != 0 && Physics.Raycast(transform.position, -transform.up, out hit, 2))
            {
                if (hit.transform.tag == "Ramp")
                    rampUnder = true;
            }
        }

        return rampUnder;
    }

    private void LatchOff()
    {
        if (PlayerInput.JumpOnce && !_fuelScript.NoFuel)
            RunLatchOff();
    }

    private void RunLatchOff()
    {
        TurnOnAirborne();

        StartCoroutine(LatchingOff());
        StartCoroutine(TurnOffClimb(1));

        if(!_fuelScript.NoFuel)
        _fuelScript.Spend(GetJumpForce);
    }

    private IEnumerator LatchingOff()
    {
        yield return new WaitForFixedUpdate();
        if(_fuelScript.NoFuel)
            _rigidbody.AddForce(transform.up * 2, ForceMode.Impulse);
        else
            _rigidbody.AddForce(transform.up * GetJumpForce, ForceMode.Impulse);
    }

    private IEnumerator Jet()
    {
        yield return new WaitForFixedUpdate();

        if (PlayerInput.JumpHold && !_fuelScript.NoFuel)
        {
            float jetPressure = GetJumpForce / 200;
            _rigidbody.AddForce(transform.up * jetPressure, ForceMode.Impulse);

            _fuelScript.Spend(jetPressure * 1000);
            _jetting = true;
            if (_thruster.isStopped)
                _thruster.Play();
        }
        else
        {
            if (_thruster.isPlaying)
                _thruster.Stop();
            _jetting = false;
        }
    }

    private void WallPush()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float push = GetJumpForce / 500;
            _rigidbody.AddForce(transform.up * push, ForceMode.VelocityChange);
            _fuelScript.Spend(push);
            if (_thruster.isStopped)
                _thruster.Play();
        }
        else
        {
            if (_thruster.isPlaying)
                _thruster.Stop();
        }
    }

    private void Decelerate()
    {
        if (PlayerInput.Gas == 0 || !Grounded())
        {
            float decelerationSpeed = acceleration / (Grounded() ? 10 : 10);
            decelerationSpeed *= _carState == CarState.Climbing ? maxSpeedClimb : 1;
            Deceleration(decelerationSpeed);
        }
    }

    private void Deceleration(float decelerationSpeed)
    {
        if (_driveState == DriveState.Accelerate)
        {
            _speed -= decelerationSpeed * (Time.deltaTime * 100);
            if (_speed < 0)
                _speed = 0;
        }
        if (_driveState == DriveState.Decelerate)
        {
            _speed += decelerationSpeed * (Time.deltaTime * 100);
            if (_speed > 0)
                _speed = 0;
        }
    }

    private void DriveStateHandler()
    {
        if (_speed > 0)
            _driveState = DriveState.Accelerate;
        else if (_speed < 0)
            _driveState = DriveState.Decelerate;
        else
            _driveState = DriveState.Park;
    }

    private void Turbo()
    {
        if (_driveState == DriveState.Accelerate && _speed >= GetMaxSpeed)
            _turbo = PlayerInput.TurboHold && !PlayerInput.TurboRelease;
        else
            _turbo = false;
    }

    private void HandleTurbo()
    {
        if (_turbo)
        {
            if (_exhaust.isStopped)
                _exhaust.Play();
            _fuelScript.AddSpending(300);

            if (_turbo)
                Tutorial.GetInstance.PlayTutorial("MaxBoost");
        }
        else
        {
            if (_exhaust.isPlaying)
                _exhaust.Stop();
        }
    }

    private void HandBrake()
    {
        if (PlayerInput.HandBrake && Grounded())
            Deceleration(acceleration * 4);
    }

    private void Accelerate()
    {
        _speed += PlayerInput.Gas * acceleration * (Time.deltaTime * 100);
    }

    private void MoveCar()
    {
        if (_drifting)
        {
            if (_oldForward.normalized != transform.forward.normalized)
            {
                Vector3 steerDir = Vector3.zero;
                if (Vector3.Angle(_oldForward.normalized, transform.right.normalized) < 88)
                    steerDir = -transform.right;
                else if (Vector3.Angle(_oldForward.normalized, transform.right.normalized) > 92)
                    steerDir = transform.right;
                _oldForward += transform.forward.normalized + steerDir;
            }
            transform.position += _oldForward.normalized * _speed * Time.deltaTime;
        }
        else
            transform.position += transform.forward * _speed * Time.deltaTime;
    }

    private void MinMaxSpeedLimit()
    {
        bool noFuel = _fuelScript.NoFuel;
        float maxSpeed = (noFuel ? GetMaxSpeed / 5 : (_turbo ? GetMaxTurboSpeed : GetMaxSpeed));
        if (_speed > maxSpeed)
            _speed = maxSpeed;
        if (_speed < -maxSpeed)
            _speed = -maxSpeed;
    }

    private void Steer(bool drift = false)
    {
        if (_speed != 0 || _carState == CarState.Climbing)
            transform.Rotate(0, Input.GetAxis("Horizontal") * 200 * Time.deltaTime, 0);
    }

    void OnCollisionStay(Collision col)
    {
        if (col.transform.name == "Platform")
        {
            StartCoroutine(MoveWithPlatform(col.transform));
        }
    }

    private IEnumerator MoveWithPlatform(Transform platform)
    {
        yield return new WaitForFixedUpdate();
        MovePlatform mp = platform.parent.GetComponent<MovePlatform>();
        float speed = mp.GetSpeed / 4;
        transform.position += (mp.NextPosition - transform.position).normalized * speed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision col)
    {
        if (_carState == CarState.StandBy || _stasisScript.ChargedStasis)
            return;
        
        bool wall = col.transform.tag == "Climbable";
        bool terrain = col.transform.tag == "Terrain";
        if ((wall || (terrain && !PlayerInput.TurboHold)) && _carState != CarState.Climbing && _canClimb)
        {
            RaycastHit hit;
            if (Grounded() && wall)
            {
                if (Physics.Raycast(transform.position, transform.forward, out hit, 2))
                {
                    bool straightWall = Vector3.Angle(Vector3.down, hit.normal) == 90;
                    bool under90Degrees = Vector3.Angle(Vector3.down, hit.normal) < 90;
                    bool over90Degrees = Vector3.Angle(Vector3.down, hit.normal) > 90 && Vector3.Angle(Vector3.down, hit.normal) <= 110;
                    if (straightWall)
                    {
                        transform.position += Vector3.up;
                        ClimbSurface(hit);
                    }
                    else if (under90Degrees)
                        ClimbSurface(hit);
                    else if (over90Degrees)
                        ClimbSurface(hit);
                }
            }
            else if (!Grounded())
            {
                Vector3 hitPoint = col.contacts[0].point;
                float distance = Vector3.Distance(transform.position, hitPoint) + 1;
                if (Physics.Raycast(transform.position, (hitPoint - transform.position).normalized, out hit, distance) &&
                    ((terrain && Vector3.Angle(hitPoint.normalized, hit.normal) > 90) || (wall && hit.normal != Vector3.down)))
                {
                    ClimbSurface(hit);
                }
            }
        }
        else if (col.transform.name.Contains("MagnetableSphere"))
            _rigidbody.AddForce(col.gameObject.GetComponent<Rigidbody>().velocity, ForceMode.VelocityChange);
        else if (col.transform.name.Contains("Magnet"))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -transform.up, out hit, 3) && !hit.transform.name.Contains("Magnet"))
                BumpOnWall();
            else
                TurnOnDriving();
        }
        else if (col.transform.tag == "Untagged")
        {
            if (Physics.Raycast(transform.position, transform.forward, 2) || Physics.Raycast(transform.position, -transform.forward, 2))
                BumpOnWall();
        }
        if(col.transform.tag == "Ramp")
            Tutorial.GetInstance.PlayTutorial("Ramp");
    }

    private void BumpOnWall()
    {
        float min = 0.5f;
        float max = _carState == CarState.Climbing ? maxSpeedClimb : maxSpeedDrive;
        _speed = (_speed > 0 ? -Mathf.Clamp(_speed, min, max) : Mathf.Clamp(-_speed, min, max)) / 2;
    }

    public void ForceShowRunesHelp()
    {
        _helpRunes.SetActive(true);
        SHOW_RUNES = true;
    }

    public void TurnOffFireEffects()
    {
        _thruster.Stop();
        _exhaust.Stop();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Animate")
        {
            _enterCinematic = true;
            _cinematicTrigger = col.transform;
            _speed = 0;
        }
        if (col.name.Contains("Explosion"))
            RunLatchOff();
        if (col.name == "Killzone" && !RuneHandler.RUNES_UNLOCKED)
            HandleDeath();
    }

    public bool ControlCar
    {
        set
        {
            _carState = value ? CarState.Driving : CarState.StandBy;
        }
        get { return _carState != CarState.StandBy; }
    }

    public bool EnterCinematic
    {
        set { _enterCinematic = value; }
        get { return _enterCinematic; }
    }

    public bool StartTeleport
    {
        get { return _teleport; }
    }

    public Transform GetCinematicTrigger
    {
        get { return _cinematicTrigger; }
    }

    private float GetMaxSpeed
    {
        get { return _carState == CarState.Climbing ? maxSpeedClimb : maxSpeedDrive; }
    }

    private float GetMaxTurboSpeed
    {
        get { return maxSpeedDrive + maxTurboSpeedAdditive; }
    }

    private float GetJumpForce
    {
        get { return _rigidbody.mass * jumpForceMultiplied; }
    }

    public float Speed
    {
        set { _speed = value; }
        get { return _speed; }
    }

    public Vector3 StartPosition
    {
        set { _startPosition = value; }
    }

    public Vector3 StartPositionOverworld
    {
        set
        {
            bool copy = false;
            foreach(Vector3 v in START_POSITIONS)
            {
                if(value == v)
                {
                    copy = true;
                    break;
                }
            }
            
            if(!copy)
                START_POSITIONS.Add(value);
        }
    }

    public Vector3 ReplaceStartPosition
    {
        set { START_POSITIONS[0] = value; }
    }

    public Vector3 StartRotation
    {
        set { _startRotation = value; }
        get { return _startRotation; }
    }

    public Vector3 AirDirection
    {
        set { _oldForward = value; }
        get { return _oldForward; }
    }

    public bool IsDead
    {
        get { return _carState == CarState.Dead; }
    }

    public bool IsAirborne
    {
        get { return _carState == CarState.Airborne; }
    }

    public bool IsClimbing
    {
        get { return _carState == CarState.Climbing; }
    }
}
