/*
 * ----------------------------------------------------------------------------
 * "THE BEER-WARE LICENSE" (Revision 42):
 * <netboy0524@gmail.com> wrote this file. As long as you retain this notice you
 * can do whatever you want with this stuff. If we meet some day, and you think
 * this stuff is worth it, you can buy me a beer in return Sangmin Park
 * ----------------------------------------------------------------------------
 */
using System;
using Cinemachine;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Playables;
public class PlayerController_Criminal : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the Character")]
    private float _walkSpeed = (float)ModelManager._criminalMovement.moveSpeed / 10000;
    [Tooltip("run speed of the Player")]
    private float _runSpeed = ModelManager._criminalMovement.runSpeed;
    [Header("Camera")]

    [Tooltip("target of the camera chasing with")]
    public GameObject _cameraTarget;
    public GameObject _minimapCameraTarget;
    public GameObject _tpsCameraTarget;
    public GameObject _spectateCopTarget;
    public GameObject _spectateSpyTarget;
    public GameObject _spectateBoomerTarget;
    public GameObject _spectateAssassinTarget;
    private List<GameObject> _spectateCameraTargets = new List<GameObject>();

    [Tooltip("Camere FOV")]
    [Range(50.0f, 90.0f)]
    private float _cameraFOV = ModelManager._criminalMovement.FOV;

    [Tooltip("Mouse sensitivity")]
    [Range(0.0f, 10.0f)]
    private float _mouseSens = ModelManager._criminalMovement.rotateSpeed;

    [Tooltip("TPS camera")]
    public GameObject _cinemachineCam;
    public CinemachineVirtualCamera _copSpectateCam;
    public CinemachineVirtualCamera _spySpectateCam;
    public CinemachineVirtualCamera _boomerSpectateCam;
    public CinemachineVirtualCamera _assassinSpectateCam;
    private List<CinemachineVirtualCamera> _spectateCams = new List<CinemachineVirtualCamera>();
    public CinemachineVirtualCamera _shotCamera;
    public CinemachineVirtualCamera _aimCam;
    private GameObject _shotTargetCam;
    public GameObject _shotSpyCam;
    public GameObject _shotBoomerCam;
    public GameObject _shotAssassinCam;
    public GameObject _crossHair;

    [Header("Animation")]
    public Animator _anim;
    [Header("Canvas")]
    [SerializeField] private List<GameObject> _jobCanvas;
    public GameObject _copCanvas;
    public GameObject _criminalCanvas;
    public TextMeshProUGUI _leftCriminal;
    public GameObject _normalAlert;
    public GameObject _reportAlert;
    public GameObject _dieCanvas;
    public GameObject _SpyPicture;
    public GameObject _BoomerPicture;
    public GameObject _AssassinPicture;
    [Header("Gun")]
    public GameObject _gun;
    [Header("Mission")]
    public GameObject _missionController;
    [Header("Report")]
    public GameObject _reportKey;
    public Image _reportImg;
    public TextMeshProUGUI _reportCoolTime;
    //player
    private rVector3 _startPos;
    private rVector3 _startRot;
    private float _horizonInput = 0.0f;
    private float _verticalInput = 0.0f;
    private float _playerRot = 0.0f;
    private float _playerVel = 0.0f;
    private float _rotVelocity;
    private CharacterController _playerController;
    private Transform _playerTF;
    //camera
    private bool _isSpectating = false;
    private int _spectatingTarget = 0;
    private float _cinemachineYaw;
    private float _cinemachinePitch;
    private GameObject _mainCamera;
    private int _priority = 13;
    //animation
    private bool _hasJustStopped = true;
    private bool _duringMotion = false;
    //Report
    private bool _whileReport = false;
    private Ray _ray;
    private RaycastHit _targetHit;
    private GameObject _targetObject;
    private Transform _focused;
    private bool _justReport = false;
    private string _reportUsername;
    private string _reportTargetUsername;
    private long _nextReportAvailAt;
    private long _highlightStartAt;
    private long _highlightEndAt;
    private string _reportMsg;
    //Stun and Shot
    private string _stunTarget;
    private string _shotTarget;
    private bool _hasShot;
    private string _shotTargetUserType;
    private int _aliveUserCount = ModelManager._leftCriminal;
    //Mission
    private bool _whileChange = false;
    private bool _hasKilled = false;
    private static readonly string _stunResponseName = responseType.STUN.ToString();
    private static readonly string _shotResponseName = responseType.SHOT.ToString();
    private static readonly string _missionResponseName = responseType.PLAY_MISSION.ToString();
    private static readonly string _KillResponseName = responseType.ASSASSIN_KILL.ToString();
    private static readonly string _ReportResponseName = responseType.REPORT_USER.ToString();
    private void Awake()
    {
        //메인카메라 기준으로 움직이기 위해 가져오기
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        if (ModelManager._userType != roomUserType.USER.ToString())
        {
            gameObject.GetComponent<PlayerController_Criminal>().enabled = false;
        }
        GetComponent<PlayableDirector>().enabled = !ModelManager._isReconnected;
    }
    // Start is called before the first frame update
    void Start()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.startListenOnMessage(StunUpdated, _stunResponseName);
        networking.startListenOnMessage(ShotUpdated, _shotResponseName);
        networking.startListenOnMessage(MissionUpdated, _missionResponseName);
        networking.startListenOnMessage(KilledUpdated, _KillResponseName);
        networking.startListenOnMessage(ReportUpdated, _ReportResponseName);

        _playerController = GetComponent<CharacterController>();
        _cinemachineYaw = _tpsCameraTarget.transform.rotation.eulerAngles.y;
        _anim = GetComponent<Animator>();
        _playerTF = GetComponent<Transform>();
        //임시 시작위치
        if (ModelManager._crimeType == crimeType.SPY.ToString())
        {
            _startPos = ModelManager._spyPosition;
            _startRot = ModelManager._spyRotation;
            _shotTargetCam = _shotSpyCam;
            _SpyPicture.SetActive(true);
        }
        else if (ModelManager._crimeType == crimeType.BOOMER.ToString())
        {
            _startPos = ModelManager._boomerPosition;
            _startRot = ModelManager._boomerRotation;
            _shotTargetCam = _shotBoomerCam;
            _BoomerPicture.SetActive(true);
        }
        else if (ModelManager._crimeType == crimeType.ASSASSIN.ToString())
        {
            _startPos = ModelManager._assassinPosition;
            _startRot = ModelManager._assassinRotation;
            _shotTargetCam = _shotAssassinCam;
            _AssassinPicture.SetActive(true);
        }

        _playerTF.position = new Vector3(_startPos.x, _startPos.y, _startPos.z);
        _playerTF.rotation = Quaternion.Euler(new Vector3(_startRot.x, _startRot.y, _startRot.z));
        //남은 범죄자초기화
        _leftCriminal.text = "X " + _aliveUserCount;
        //범죄자용 캔버스 비활성화
        _copCanvas.SetActive(false);
        _dieCanvas.SetActive(false);
    }
    void OnDestroy()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.stopListenOnMessage(StunUpdated, _stunResponseName);
        networking.stopListenOnMessage(ShotUpdated, _shotResponseName);
        networking.stopListenOnMessage(MissionUpdated, _missionResponseName);
        networking.stopListenOnMessage(KilledUpdated, _KillResponseName);
        networking.stopListenOnMessage(ReportUpdated, _ReportResponseName);
    }
    // Update is called once per frame
    void Update()
    {
        ToggleUI();

        PlayerReplace();
        if (!ModelManager._isDisconnected)
        {
            //죽으면 끝
            if (!ModelManager._isDead)
            {
                //미션중에는 이동, 행동 불가
                if (!ModelManager._whileMission && !_whileChange)
                {
                    //죽이는 모션중 이동불가
                    if (!_hasKilled)
                    {
                        //조준시 이동불가, 행동만 가능 요청은 계속 보냄
                        if (ModelManager._isStunned)
                        {
                            AlertMsg("경찰에게 <color=\"red\">검문</color> 당했습니다!\n특수 모션을 실행해 NPC인 척 하세요!");
                            Debug.Log("검문중!");
                        }
                        else
                        {
                            CameraChange();
                            AlertMsg("");
                            PlayerMove();
                            CriminalReport();
                            ChangeCharacter();
                        }
                        PlayerAnim();
                    }
                    else
                    {
                        //코루틴으로 2초간 죽이는 모션실행
                        StartCoroutine("KillMotion");
                    }
                }
                else
                {
                    _anim.SetInteger("motion", 8);
                }
                Request();
                ReportResult();
            }
            else
            {
                //미션 비활성화
                _missionController.SetActive(false);
                Spectate();
            }
            //사격당할시 연출과 범죄자 감소
            if (_hasShot)
            {
                if (_shotTargetUserType == roomUserType.USER.ToString())
                {
                    if (_shotTarget == ModelManager._username)
                    {
                        StartCoroutine("CriminalShotMotion");
                    }
                    else
                    {
                        StartCoroutine("CriminalShot");
                    }
                }
                else
                {
                    _gun.GetComponent<Animator>().SetTrigger("Shot");
                }
                _hasShot = false;
            }
            _leftCriminal.text = "X " + _aliveUserCount;
        }
    }
    private void LateUpdate()
    {
        //시점카메라는 움직임과 동시에 Update하면 오류가 발생할수 있으므로 늦게 update
        //죽어도 카메라, root 변경해서 카메라 조작은 가능하도록

        CameraRot();
    }
    // 자기 캐릭터 조작
    private void PlayerMove()
    {
        if (!ModelManager._whileChat)
        {
            _horizonInput = Input.GetAxisRaw("Horizontal");
            _verticalInput = Input.GetAxisRaw("Vertical");

            Vector3 inputDir = new Vector3(_horizonInput, 0.0f, _verticalInput).normalized;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                // 플레이어의 방향 = 카메라 기준 키보드 입력방향
                // 자유시점은 사망 후 시점이라 관계 X
                _playerRot = _mainCamera.transform.eulerAngles.y;
                _playerRot = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;

                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _playerRot, ref _rotVelocity, 0.1f);
                //회전을 먼저 시킨후
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
            //플레이어의 정면 방향으로 이동
            Vector3 playerDir = Quaternion.Euler(0.0f, _playerRot, 0.0f) * Vector3.forward;
            //경찰은 조준시 움직임 제한
            if (_horizonInput != 0 || _verticalInput != 0)
            {
                _playerVel = Input.GetKey(KeyCode.LeftShift) ? _runSpeed : _walkSpeed;
                _playerController.Move(playerDir.normalized * (_playerVel * Time.deltaTime));
            }
        }
    }
    private void PlayerAnim()
    {

        //스턴시 일단 멈춤
        if (ModelManager._hasJustStunned)
        {
            MotionInit();
            _playerVel = 0.0f;
            _anim.SetInteger("motion", 0);
            _duringMotion = false;
            ModelManager._hasJustStunned = false;
        }

        if (!ModelManager._isStunned && (_horizonInput != 0 || _verticalInput != 0) && !ModelManager._whileChat)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                _anim.SetInteger("motion", 2);
                _playerVel = _runSpeed;
            }
            else
            {
                _anim.SetInteger("motion", 1);
                _playerVel = _walkSpeed;
            }
            //애니메이션(motion)
            //3,4,5 : 이모트 / 0 : idle / 1 : walk / 2 : run
            MotionInit();
        }
        else
        {
            //나중에 modelmanager 모션값으로 변경하기
            if (_hasJustStopped)
            {
                _playerVel = 0.0f;
                _anim.SetInteger("motion", 0);
                _hasJustStopped = false;
            }
            if (!ModelManager._whileChat)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) && !_duringMotion)
                {
                    _anim.SetInteger("motion", 3);
                    _duringMotion = true;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2) && !_duringMotion)
                {
                    _anim.SetInteger("motion", 4);
                    _duringMotion = true;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3) && !_duringMotion)
                {
                    _anim.SetInteger("motion", 5);
                    _duringMotion = true;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4) && !_duringMotion)
                {
                    _anim.SetInteger("motion", 6);
                    _duringMotion = true;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha5) && !_duringMotion)
                {
                    _anim.SetInteger("motion", 7);
                    _duringMotion = true;
                }
            }
        }
    }
    private void MotionInit()
    {
        _duringMotion = false;
        _hasJustStopped = true;
    }
    private void Spectate()
    {
        //모든캐릭터 외각선 활성화 및 관전캠 추가
        if (Input.GetKey(KeyCode.Space) && !_isSpectating)
        {
            RoomInfo._reportTarget[4] = true;
            _spectateCams.Add(_copSpectateCam);
            _spectateCameraTargets.Add(_spectateCopTarget);
            //각직업이 게임에있으면
            if (ModelManager._spyNickname != null)
            {
                RoomInfo._reportTarget[1] = true;
                _spectateCams.Add(_spySpectateCam);
                _spectateCameraTargets.Add(_spectateSpyTarget);
            }
            if (ModelManager._boomerNickname != null)
            {
                RoomInfo._reportTarget[2] = true;
                _spectateCams.Add(_boomerSpectateCam);
                _spectateCameraTargets.Add(_spectateBoomerTarget);
            }
            if (ModelManager._assasssinNickname != null)
            {
                RoomInfo._reportTarget[3] = true;
                _spectateCams.Add(_assassinSpectateCam);
                _spectateCameraTargets.Add(_spectateAssassinTarget);
            }
            _aimCam.enabled = false;
            _cinemachineCam.SetActive(false);
            _criminalCanvas.SetActive(false);
            AlertMsg("[ " + ModelManager._copNickname + " ] 관전 중");
            //카메라 이동
            SwitchCamera(_spectatingTarget);
            _isSpectating = true;
        }
        if (Input.GetMouseButtonDown(0) && _isSpectating)
        {
            if (_spectatingTarget == _spectateCams.Count - 1)
                _spectatingTarget = 0;
            else
                _spectatingTarget++;
            SwitchCamera(_spectatingTarget);
        }
    }
    //해당 관전카메라로 이동
    private void SwitchCamera(int i)
    {
        for (int j = 0; j < _spectateCams.Count; j++)
        {
            _spectateCams[j].enabled = j == i;
            if (j == i)
            {
                _cameraTarget = _spectateCameraTargets[j];
                AlertMsg("[ " + _cameraTarget.transform.parent.GetComponent<PlayerInfo>()._roomUser.nickname + " ] 관전 중");
            }
        }
    }
    private void ChangeCharacter()
    {
        if (Input.GetKeyDown(KeyCode.Z) && !ModelManager._whileChat)
        {
            StartCoroutine("ChangeOutfit");
            //임시 의상체인지 모션
            _whileChange = true;
        }
    }
    IEnumerator ChangeOutfit()
    {
        AlertMsg("변장 중...");
        yield return new WaitForSeconds(3);
        AlertMsg("");
        //임시 의상체인지 모션 종료
        _whileChange = false;
        _hasJustStopped = true;
        //색 바꿔서 패킷보내기
        int _prevColor = ModelManager._userColor;
        while (_prevColor == ModelManager._userColor)
        {
            ModelManager._userColor = UnityEngine.Random.Range(0, 4);
        }
    }
    // 자기 카메라 조작
    private void CameraRot()
    {
        //마우스 이동에 따라 수직수평값 변경, 수평감도만 추가
        _cinemachineYaw += Input.GetAxis("Mouse X") * 1.0f * _mouseSens;
        _cinemachinePitch -= Input.GetAxis("Mouse Y") * 0.5f;

        //마우스 rotation 항상 -360~360도로 유지, 마우스 상하 이동 아래로 30도, 위로 70도로 제한
        _cinemachineYaw = ClampAngle(_cinemachineYaw, float.MinValue, float.MaxValue);
        _cinemachinePitch = ClampAngle(_cinemachinePitch, -30.0f, 70.0f);

        //카메라 타겟을 회전시켜 카메라가 타겟 중심으로 회전하는것처럼 보이도록
        _cameraTarget.transform.rotation = Quaternion.Euler(_cinemachinePitch, _cinemachineYaw, 0.0f);
        _minimapCameraTarget.transform.rotation = Quaternion.Euler(0, _cinemachineYaw, 0);
    }
    // 카메라 설정
    private void CameraChange()
    {
        //FOV 값 조절
        _cinemachineCam.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = _cameraFOV;
        //단축키 T로 cinemachine 우선순위 조절하여 자유시점 이동가능
        //우클릭시 조준시점 카메라로
        _priority = _whileReport ? 10 : 13;
        _cinemachineCam.GetComponent<CinemachineVirtualCamera>().Priority = _priority;
        //조준점 on/off
        _crossHair.SetActive(_whileReport);
    }
    private static float ClampAngle(float Angle, float Min, float Max)
    {
        return Mathf.Clamp(NormalizeAngles(Angle), Min, Max);
    }
    public static float NormalizeAngles(float Angle)
    {
        if (Angle < -360.0f) Angle += 360.0f;
        if (Angle > 360.0f) Angle -= 360.0f;
        return Angle;
    }
    private void ToggleUI()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (ModelManager._crimeType == crimeType.SPY.ToString())
            {
                if (_jobCanvas[0].activeSelf)
                    _jobCanvas[0].SetActive(false);
                else
                    _jobCanvas[0].SetActive(true);
            }
            if (ModelManager._crimeType == crimeType.BOOMER.ToString())
            {
                if (_jobCanvas[1].activeSelf)
                    _jobCanvas[1].SetActive(false);
                else
                    _jobCanvas[1].SetActive(true);
            }
            if (ModelManager._crimeType == crimeType.ASSASSIN.ToString())
            {
                if (_jobCanvas[2].activeSelf)
                    _jobCanvas[2].SetActive(false);
                else
                    _jobCanvas[2].SetActive(true);
            }
        }
    }
    private void AlertMsg(string msg)
    {
        _normalAlert.SetActive(msg != "");
        _normalAlert.GetComponentInChildren<TextMeshProUGUI>().text = msg;
    }
    private void ReportMsg(string msg)
    {
        _reportAlert.SetActive(msg != "");
        _reportAlert.GetComponentInChildren<TextMeshProUGUI>().text = msg;
    }
    //신고
    private void CriminalReport()
    {
        //오른클릭 - START
        if (Input.GetMouseButtonDown(1) && !_whileReport && !ModelManager._whileChat)
        {
            StartReport();
        }
        //오른클릭 - END
        else if (Input.GetMouseButtonDown(1) && _whileReport && !ModelManager._whileChat)
        {
            EndReport();
        }
        //PROCESS
        else if (_whileReport)
        {
            WhileReport();
        }
        //None
        else if (!_whileReport)
        {
            EndReport();
        }
    }
    private void ReportResult()
    {
        //신고 결과
        if (_justReport)
        {
            //내가 신고했을떄
            if (_reportUsername == ModelManager._username)
            {
                StartCoroutine(ReportCoolTime(_nextReportAvailAt));
                //신고 실패시 내위치 발각
                if (_reportTargetUsername == ModelManager._username)
                {
                    StartCoroutine(ReportResultMsg(_reportMsg, _highlightStartAt, _highlightEndAt));
                }
            }
            //다른 범죄자가 신고했을떄
            else
            {
                if (_reportTargetUsername == ModelManager._username)
                {
                    StartCoroutine(ReportResultMsg(_reportMsg, _highlightStartAt, _highlightEndAt));
                }
            }
            _justReport = false;
        }
    }
    public void StartReport()
    {
        _whileReport = true;
    }
    public void WhileReport()
    {
        if (_focused != null)
        {
            if (_targetObject != null)
            {
                _targetObject.GetComponent<Outline>().enabled = false;
                _reportKey.SetActive(false);
            }
            _focused = null;
        }
        _ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        // 어떤것과 충돌하면 해당 좌표 저장
        if (Physics.Raycast(_ray, out _targetHit, ModelManager._report.ReportRange))
        {
            _targetObject = _targetHit.transform.parent.gameObject;
            //상대가 캐릭터면고 내가 아닐때
            if (_targetObject.layer == LayerMask.NameToLayer("Character"))
            {
                string targetName = _targetObject.GetComponent<PlayerInfo>()._roomUser.username;
                if (ModelManager._users[targetName].roomUserType != roomUserType.COP.ToString()
                && targetName != ModelManager._username)
                {
                    _targetObject.GetComponent<Outline>().enabled = true;
                    _reportKey.SetActive(true);
                    //경찰이 아닌 상태에게 R를 눌렀을 경우
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        sendReportRequest(ModelManager._username, targetName);
                    }
                    _focused = _targetHit.transform;
                }
            }
        }
    }
    public void EndReport()
    {
        if (_targetObject != null)
        {
            if (_targetObject.layer == LayerMask.NameToLayer("Character"))
            {
                _targetObject.GetComponent<Outline>().enabled = false;
                _reportKey.SetActive(false);
            }
        }
        _whileReport = false;
    }
    /// <summary>
    /// 재접속 데이터
    /// </summary>
    private void PlayerReplace()
    {
        if (ModelManager._isReconnected)
        {
            //위치, 방향 업데이트
            if (ModelManager._crimeType == crimeType.SPY.ToString())
            {
                Vector3 pos = new Vector3(RoomInfo._roomSpy.pos.x, 0, RoomInfo._roomSpy.pos.z);
                Quaternion rot = Quaternion.Euler(RoomInfo._roomSpy.rot.x, RoomInfo._roomSpy.rot.y, RoomInfo._roomSpy.rot.z);
                GetComponent<Transform>().position = pos;
                GetComponent<Transform>().rotation = rot;
            }
            if (ModelManager._crimeType == crimeType.ASSASSIN.ToString())
            {
                Vector3 pos = new Vector3(RoomInfo._roomAssassin.pos.x, 0, RoomInfo._roomAssassin.pos.z);
                Quaternion rot = Quaternion.Euler(RoomInfo._roomAssassin.rot.x, RoomInfo._roomAssassin.rot.y, RoomInfo._roomAssassin.rot.z);
                GetComponent<Transform>().position = pos;
                GetComponent<Transform>().rotation = rot;
            }
            if (ModelManager._crimeType == crimeType.BOOMER.ToString())
            {
                Vector3 pos = new Vector3(RoomInfo._roomBoomer.pos.x, 0, RoomInfo._roomBoomer.pos.z);
                Quaternion rot = Quaternion.Euler(RoomInfo._roomBoomer.rot.x, RoomInfo._roomBoomer.rot.y, RoomInfo._roomBoomer.rot.z);
                GetComponent<Transform>().position = pos;
                GetComponent<Transform>().rotation = rot;
            }

            //신고 메시지 띄우기
            if (ReconnectionController._recentReportForCriminal.Length != 0)
            {
                foreach (ReportInfo report in ReconnectionController._recentReportForCriminal)
                {
                    _reportUsername = report.reportUsername;
                    _reportTargetUsername = report.targetUsername;
                    _nextReportAvailAt = Convert.ToInt64(report.nextReportAvailAt);
                    _highlightStartAt = Convert.ToInt64(report.highlightStartAt);
                    _highlightEndAt = Convert.ToInt64(report.highlightEndAt);
                    _reportMsg = report.reportMessage;
                    //내가 신고했을떄
                    if (_reportUsername == ModelManager._username)
                    {
                        StartCoroutine(ReportCoolTime(_nextReportAvailAt));
                        //신고 실패시 내위치 발각
                        if (_reportTargetUsername == ModelManager._username)
                        {
                            StartCoroutine(ReportResultMsg(_reportMsg, _highlightStartAt, _highlightEndAt));
                        }
                    }
                    //다른 범죄자가 신고했을떄
                    else
                    {
                        if (_reportTargetUsername == ModelManager._username)
                        {
                            StartCoroutine(ReportResultMsg(_reportMsg, _highlightStartAt, _highlightEndAt));
                        }
                    }
                }
                ReconnectionController._recentReportForCriminal = new ReportInfo[0];
            }
        }
    }
    /// <summary>
    /// 신고 쿨타임
    /// </summary>
    IEnumerator ReportCoolTime(long nextReportAvailAt)
    {

        _reportImg.fillAmount = 0;
        _reportCoolTime.gameObject.SetActive(true);
        float coolTime = (nextReportAvailAt - PingManager.GameTime) / 1000.0f;
        float fullCoolTime = coolTime;
        while (coolTime >= 0)
        {
            coolTime -= Time.deltaTime;

            _reportCoolTime.text = string.Format("{0}", (int)coolTime);
            _reportImg.fillAmount = 1.0f - (coolTime / fullCoolTime);

            yield return new WaitForFixedUpdate();
        }
        _reportImg.fillAmount = 1;
        _reportCoolTime.gameObject.SetActive(false);
    }
    //신고 결과 출력
    IEnumerator ReportResultMsg(string reportMsg, long highlightStartAt, long highlightEndAt)
    {
        float reportStartTime = (highlightStartAt - PingManager.GameTime) / 1000.0f;
        float reportEndTime = (highlightEndAt - PingManager.GameTime) / 1000.0f;
        ReportMsg("");
        while (reportEndTime >= 0)
        {
            reportEndTime -= Time.deltaTime;
            reportStartTime -= Time.deltaTime;
            if (reportStartTime < 0)
            {
                int focusTime = (int)Mathf.Ceil(reportEndTime);
                ReportMsg(reportMsg + "\n<color=\"red\">" + focusTime.ToString());
            }
            yield return new WaitForFixedUpdate();
        }
        ReportMsg("");
    }
    //본인 사망시
    IEnumerator CriminalShotMotion()
    {
        AlertMsg("");
        float motionTime = 0f;
        bool shotOnce = true;
        _shotCamera.Priority = 14;
        while (motionTime < 3)
        {
            motionTime += Time.deltaTime;
            float x = Mathf.Lerp(-2000, -555, motionTime);
            _shotTargetCam.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, 0, 0);
            if (shotOnce && motionTime > 2.5)
            {
                _gun.GetComponent<Animator>().SetTrigger("Shot");
                shotOnce = false;
            }
            yield return new WaitForFixedUpdate();
        }
        _shotCamera.Priority = 0;
        _shotTargetCam.SetActive(false);
        //사망 화면
        _dieCanvas.SetActive(true);
    }
    //다른 범죄자 사망시
    IEnumerator CriminalShot()
    {
        float motionTime = 0f;
        bool shotOnce = true;
        while (motionTime < 3)
        {
            motionTime += Time.deltaTime;
            if (shotOnce && motionTime > 2.5)
            {
                _gun.GetComponent<Animator>().SetTrigger("Shot");
                shotOnce = false;
            }
            yield return new WaitForFixedUpdate();
        }
    }
    //살해 모션
    IEnumerator KillMotion()
    {
        float motionTime = 0f;
        bool animOnce = true;
        _anim.SetInteger("motion", 12);
        while (motionTime < 2)
        {
            motionTime += Time.deltaTime;
            if (animOnce && motionTime > 1.5f)
            {
                MotionInit();
                animOnce = false;
            }
            yield return new WaitForFixedUpdate();
        }
        _hasKilled = false;
    }
    public void Request()
    {
        if (ModelManager._whileGame)
        {
            NetworkingController networking = NetworkManager.getNetworkingController();
            RequestUpdateUser(networking);
        }
    }
    private void RequestUpdateUser(NetworkingController networking)
    {
        var request = new UpdateUserRequest(_playerTF, _playerVel, _anim.GetInteger("motion"));
        networking.sendRequest(request);
    }
    public void sendReportRequest(string reportUsername, string targetUsername)
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        RequestReport(networking, reportUsername, targetUsername);
    }
    private void RequestReport(NetworkingController networking, string reportUsername, string targetUsername)
    {
        var request = new ReportUserRequest(reportUsername, targetUsername);
        networking.sendRequest(request);
    }
    //Player 위치, 방향등 정보 전송하는 패킷
    public class UpdateUserRequest : ClientRequest
    {
        public RoomUser roomUser;
        public int roomId;

        public UpdateUserRequest(Transform playerTF, float playerVel, int playerAnim)
        {
            type = requestType.UPDATE_USER.ToString();
            this.roomUser = new RoomUser()
            {
                username = ModelManager._username,
                nickname = ModelManager._nickname,
                pos = new rVector3(playerTF.position.x, playerTF.position.y, playerTF.position.z),
                rot = new rVector3
                    (0.0f, playerTF.eulerAngles.y, 0.0f),
                velocity = playerVel,
                anim = playerAnim,
                color = ModelManager._userColor,
                roomUserType = ModelManager._userType,
                crimeType = ModelManager._crimeType
            };
            this.roomId = ModelManager._roomId;
        }
    }
    public class ReportUserRequest : ClientRequest
    {
        public string reportUsername;
        public string targetUsername;
        public int roomId;
        public ReportUserRequest(string reportUsername, string targetUsername)
        {
            type = requestType.REPORT_USER.ToString();
            this.reportUsername = reportUsername;
            this.targetUsername = targetUsername;
            this.roomId = ModelManager._roomId;
        }
    }
    private void StunUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        StunResponse StunInfo = StunResponse.CreateFromJSON(e.Data);
        _stunTarget = StunInfo.targetUsername;
        if (_stunTarget == ModelManager._username)
        {
            if (StunInfo.stunState == stunState.START.ToString())
            {
                ModelManager._hasJustStunned = true;
                ModelManager._isStunned = true;
            }
            if (StunInfo.stunState == stunState.END.ToString())
            {
                ModelManager._isStunned = false;
            }
        }
    }
    private void ShotUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        ShotResponse ShotInfo = ShotResponse.CreateFromJSON(e.Data);
        _shotTarget = ShotInfo.targetUsername;
        if (_shotTarget == ModelManager._username)
        {
            ModelManager._isDead = true;
        }
        _hasShot = true;
        _shotTargetUserType = ShotInfo.targetUserType;
        _aliveUserCount = ShotInfo.aliveUserCount;
    }
    private void MissionUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        PlayMissionResponse MissionInfo = PlayMissionResponse.CreateFromJSON(e.Data);
        if (MissionInfo.username == ModelManager._username)
        {
            if (MissionInfo.missionState == missionState.START.ToString())
            {
                ModelManager._whileMission = true;
            }
            else
            {
                ModelManager._whileMission = false;
                _hasJustStopped = true;
            }
        }
    }
    private void KilledUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        AssassinKillResponse KilledInfo = AssassinKillResponse.CreateFromJSON(e.Data);
        if (ModelManager._crimeType == crimeType.ASSASSIN.ToString())
            _hasKilled = true;
    }
    private void ReportUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        ReportUserResponse reportInfo = ReportUserResponse.CreateFromJSON(e.Data);
        _whileReport = false;
        _justReport = true;
        _reportUsername = reportInfo.reportUsername;
        _reportTargetUsername = reportInfo.targetUsername;
        _nextReportAvailAt = Convert.ToInt64(reportInfo.nextReportAvailAt);
        _highlightStartAt = Convert.ToInt64(reportInfo.highlightStartAt);
        _highlightEndAt = Convert.ToInt64(reportInfo.highlightEndAt);
        _reportMsg = reportInfo.reportMessage;
    }
    public class StunResponse
    {
        public string targetUsername;
        public string availShotAt;
        public string nextStunAvailAt;
        public string stunState;
        public static StunResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<StunResponse>(jsonString);
        }
    }
    public class ShotResponse
    {
        public string targetUsername;
        public string targetUserType;
        public int aliveUserCount;
        public static ShotResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<ShotResponse>(jsonString);
        }
    }
    public class PlayMissionResponse
    {
        public int roomId;
        public int missionPhase;
        public rVector3 missionPos;
        public string crimeType;
        public string username;
        public string missionState;
        public static PlayMissionResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<PlayMissionResponse>(jsonString);
        }
    }
    public class AssassinKillResponse
    {
        public string targetUsername;
        public static AssassinKillResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<AssassinKillResponse>(jsonString);
        }
    }
    public class ReportUserResponse
    {
        public string reportUsername;
        public string nextReportAvailAt;
        public string targetUsername;
        public string highlightStartAt;
        public string highlightEndAt;
        public string reportMessage;
        public static ReportUserResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<ReportUserResponse>(jsonString);
        }
    }
}