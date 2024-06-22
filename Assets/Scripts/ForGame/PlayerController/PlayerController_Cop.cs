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
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Playables;
using UnityEngine.XR;
using System.Collections.Generic;

public class PlayerController_Cop : MonoBehaviour
{
    [Header("Player")]
    private float _walkSpeed = (float)ModelManager._copMovement.moveSpeed / 10000;
    private float _runSpeed = ModelManager._copMovement.runSpeed;

    [Header("Camera")]
    [Tooltip("target of the camera chasing with")]
    public GameObject _cameraTarget;
    public GameObject _minimapCameraTarget;
    public CinemachineVirtualCamera _shotCamera;
    public GameObject _shotSpyCamera;
    public GameObject _shotBoomerCamera;
    public GameObject _shotAssassinCamera;

    [Tooltip("Camere FOV")]
    private float _cameraFOV = ModelManager._copMovement.FOV;

    [Tooltip("Mouse sensitivity")]
    private float _mouseSens = ModelManager._copMovement.rotateSpeed;

    [Tooltip("TPS camera")]
    public GameObject _cinemachineCam;
    public GameObject _crossHair;

    [Header("Animation")]
    public Animator _anim;
    [Header("Canvas")]
    [SerializeField] private GameObject _jobCanvas;
    [SerializeField] private GameObject _minimap;
    public GameObject _criminalCanvas;
    public TextMeshProUGUI _leftCriminal;
    public TextMeshProUGUI _stunCoolTime;
    public Image _stunImg;
    public TextMeshProUGUI _shotCoolTime;
    public Image _shotImg;
    public GameObject _alert;
    public GameObject _reportAlert;
    [Header("Gun")]
    public GameObject _gun;
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
    private float _cinemachineYaw;
    private float _cinemachinePitch;
    private GameObject _mainCamera;
    private int _priority = 12;
    //Aim
    private Ray _ray;
    public rVector3 _aimPos;
    public bool _whileAim = false;
    private RaycastHit _hitData;
    private GameObject _hitObj;
    private string _targetId;
    private GameObject _prevHitObj;
    //Report
    private bool _justReport = false;
    private string _reportUsername;
    private string _reportTargetUsername;
    private long _nextReportAvailAt;
    private long _highlightStartAt;
    private long _highlightEndAt;
    //Stun and Shot
    private long _nextStunAvailAt;
    private bool _isStunCool = false;
    private long _availShotAt;
    private bool _justStun = false;
    private bool _releaseStun = false;
    private string _shotTarget;
    private string _shotTargetCrimeType;
    private bool _shotOnce = true; //클릭은 한번만 입력받기
    private bool _hasShot = false; //사격한 후에 스턴해제시켜주는 flag
    private bool _afterShot = false; //사격 후에 쿨타임 적용시켜주는 flag (스턴해제시 필요하므로 구분)
    private string _shotTargetUserType;
    private int _aliveUserCount = ModelManager._leftCriminal;
    //Mission
    private bool _missionSuccess = false;
    private bool _missionFailed = false;
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
        //경찰 아닐경우 컨트롤러 비활성화
        if (ModelManager._userType != roomUserType.COP.ToString())
        {
            gameObject.GetComponent<PlayerController_Cop>().enabled = false;
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

        //필요 오브젝트 초기화
        _playerController = GetComponent<CharacterController>();
        _cinemachineYaw = _cameraTarget.transform.rotation.eulerAngles.y;
        _playerTF = GetComponent<Transform>();
        _anim = GetComponent<Animator>();
        _startPos = ModelManager._copPosition;
        _startRot = ModelManager._copRotation;

        //위치 초기화
        GetComponent<Transform>().position = new Vector3(_startPos.x, _startPos.y, _startPos.z);
        GetComponent<Transform>().rotation = Quaternion.Euler(new Vector3(_startRot.x, _startRot.y, _startRot.z));
        //남은 범죄자초기화
        _leftCriminal.text = "X " + _aliveUserCount;
        //범죄자용 캔버스 비활성화
        _criminalCanvas.SetActive(false);
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

        if (!ModelManager._isDisconnected)
        {
            //검문중이 아닐때만 행동
            if (!ModelManager._whileStun)
            {
                CameraChange();
                PlayerMove();
                PoliceAction();
                PlayerAnim();
            }
            //검문중일때는 검문 해제 혹은 사격만 가능
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //사격
                    if (_shotOnce)
                    {
                        sendRequestShot();
                        //_shotOnce = false;
                    }
                }
                if (Input.GetMouseButtonDown(1))
                {
                    //검문해제
                    EndStun();
                }
            }
            //검문후 조준해제, 사격 쿨 적용(조준선, 외각선 제거용)
            if (_justStun)
            {
                EndAim();
                _justStun = false;
                StartCoroutine("ShotCoolTime");
            }
            //사격후 메시지 출력
            if (_hasShot)
            {
                Shot();
                StartCoroutine("StunCoolTime");
                _shotOnce = true;
                _hasShot = false;
            }
            //검문 해제시 쿨타임 적용
            if (_releaseStun)
            {
                //사격 쿨타임중 검문취소시 쿨타임 초기화
                StopCoroutine("ShotCoolTime");
                ShotInit();
                StartCoroutine("StunCoolTime");
                _releaseStun = false;
                _afterShot = false;
            }
            //유저정보 전송
            Request();
            //범죄자 신고시
            CriminalReport();
            //남은 유저 출력
            _leftCriminal.text = "X " + _aliveUserCount;
            //임시 범죄자가 미션완료했다는 메시지 출력
            if (_missionSuccess)
            {
                StartCoroutine("ShotResult", "누군가 미션을 완수하였습니다...\n대상의 위치가 잠시 노출됩니다!");
                _missionSuccess = false;
            }
            //임시 범죄자가 미션실패했다는 메시지 출력
            if (_missionFailed)
            {
                StartCoroutine("ShotResult", "누군가 미션을 실패하였습니다...\n대상의 위치가 잠시 노출됩니다!");
                _missionFailed = false;
            }
        }
        PlayerReplace();
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
            //쉬프트 홀드로 걷기, 달리기
            _horizonInput = Input.GetAxisRaw("Horizontal");
            _verticalInput = Input.GetAxisRaw("Vertical");

            Vector3 inputDir = new Vector3(_horizonInput, 0.0f, _verticalInput).normalized;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || _whileAim)
            {
                // 플레이어의 방향 = 카메라 기준 키보드 입력방향
                // 자유시점은 사망 후 시점이라 관계 X
                // 경찰은 조준시 wasd비활성화
                if (_whileAim)
                {
                    _playerRot = _mainCamera.transform.eulerAngles.y;
                }
                else
                {
                    _playerRot = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                }
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _playerRot, ref _rotVelocity, 0.1f);
                //회전을 먼저 시킨후
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
            //플레이어의 정면 방향으로 이동
            Vector3 playerDir = Quaternion.Euler(0.0f, _playerRot, 0.0f) * Vector3.forward;
            //경찰은 조준시 움직임 제한
            if (!_whileAim && (_horizonInput != 0 || _verticalInput != 0))
            {
                _playerVel = Input.GetKey(KeyCode.LeftShift) ? _runSpeed : _walkSpeed;
                _playerController.Move(playerDir.normalized * (_playerVel * Time.deltaTime));
            }
        }
    }
    private void PlayerAnim()
    {
        if (!ModelManager._whileChat)
        {
            if (_whileAim || ModelManager._whileStun)
            {
                //조준 모션만
                _anim.SetInteger("motion", 9);
            }
            //이동할때
            else if (_horizonInput != 0 || _verticalInput != 0)
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
            }
            //정지시
            else
            {
                _anim.SetInteger("motion", 0);
            }
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
        //우클릭시 조준시점 카메라로
        _priority = _whileAim ? 10 : 12;
        _cinemachineCam.GetComponent<CinemachineVirtualCamera>().Priority = _priority;
        //조준점 on/off
        _crossHair.SetActive(_whileAim);
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
    private void PoliceAction()
    {
        if (!ModelManager._whileChat)
        {
            //오른클릭 - START
            if (Input.GetMouseButtonDown(1) && !_whileAim)
            {
                StartAim();
            }
            //오른클릭 - END
            else if (Input.GetMouseButtonDown(1) && _whileAim)
            {
                EndAim();
            }
            //PROCESS
            else if (_whileAim)
            {
                WhileAim();
            }
        }
    }
    public void StartAim()
    {
        sendRequestAim(aimState.START.ToString());
        _whileAim = true;
    }
    public void WhileAim()
    {
        //레이저 포인터 시작좌표, 끝좌표 설정
        sendRequestAim(aimState.PROCESS.ToString());
        _ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        // 어떤것과 충돌하면 해당 좌표 저장
        if (Physics.Raycast(_ray, out _hitData, 10))
        {
            _prevHitObj = _hitObj;
            _hitObj = _hitData.transform.gameObject;
            _aimPos = new rVector3(_hitData.point.x, _hitData.point.y, _hitData.point.z);
            //이전 오브젝트 텍스처 원상복귀
            if (_prevHitObj && _prevHitObj.layer == LayerMask.NameToLayer("Character"))
            {
                _prevHitObj.transform.parent.gameObject.GetComponent<Outline>().enabled = false;
            }
            //레이저가 character와 충돌했을경우 텍스쳐 적용
            if (_hitObj.layer == LayerMask.NameToLayer("Character"))
            {
                _hitObj.transform.parent.gameObject.GetComponent<Outline>().enabled = true;
                _targetId = _hitObj.transform.parent.GetComponent<PlayerInfo>()._roomUser.username;
                // 검문!!!!
                if (Input.GetMouseButtonDown(0))
                {
                    StartStun();
                }
            }
        }
        // 충돌안할경우 10미터앞 좌표 저장
        else
        {
            _aimPos = new rVector3(_ray.GetPoint(10).x, _ray.GetPoint(10).y, _ray.GetPoint(10).z);
            //레이저가 캐릭터를 벗어났을경우 텍스쳐 원상복귀
            if (_prevHitObj && _prevHitObj.layer == LayerMask.NameToLayer("Character"))
            {
                _prevHitObj.transform.parent.gameObject.GetComponent<Outline>().enabled = false;
            }
        }
    }
    public void EndAim()
    {
        //캐릭터 위에서 마우스를 놨을경우 텍스쳐 원상복귀
        if (_hitObj && _hitObj.layer == LayerMask.NameToLayer("Character"))
        {
            _hitObj.transform.parent.gameObject.GetComponent<Outline>().enabled = false;
        }
        sendRequestAim(aimState.END.ToString());
        _whileAim = false;
    }
    public void StartStun()
    {
        if (_isStunCool)
        {
            Debug.Log("스턴 재사용 대기시간이 남았습니다");
        }
        else
        {
            sendRequestStun(_targetId, stunState.START.ToString());
        }
    }
    public void EndStun()
    {
        sendRequestStun(_targetId, stunState.END.ToString());
    }
    public void CriminalReport()
    {
        if (_justReport)
        {
            _justReport = false;
            if (ModelManager._users[_reportTargetUsername].crimeType == crimeType.SPY.ToString())
            {
                StartCoroutine(ReportResult(1, _highlightStartAt, _highlightEndAt));
            }
            if (ModelManager._users[_reportTargetUsername].crimeType == crimeType.BOOMER.ToString())
            {
                StartCoroutine(ReportResult(2, _highlightStartAt, _highlightEndAt));
            }
            if (ModelManager._users[_reportTargetUsername].crimeType == crimeType.ASSASSIN.ToString())
            {
                StartCoroutine(ReportResult(3, _highlightStartAt, _highlightEndAt));
            }
        }
    }
    //쿨타임 코루틴
    IEnumerator StunCoolTime()
    {
        Debug.Log("start coroutine");
        _isStunCool = true;
        _stunImg.fillAmount = 0;
        _stunCoolTime.gameObject.SetActive(true);
        float coolTime = (_nextStunAvailAt - PingManager.GameTime) / 1000.0f;
        float fullCoolTime = coolTime;
        while (coolTime >= 0)
        {
            coolTime -= Time.deltaTime;

            _stunCoolTime.text = string.Format("{0}", (int)coolTime);
            _stunImg.fillAmount = 1.0f - (coolTime / fullCoolTime);

            yield return new WaitForFixedUpdate();
        }
        StunInit();
        _isStunCool = false;
    }
    public void StunInit()
    {
        _stunImg.fillAmount = 1;
        _stunCoolTime.gameObject.SetActive(false);
    }
    IEnumerator ShotCoolTime()
    {
        Debug.Log("start coroutine");
        _shotImg.fillAmount = 0;
        _shotCoolTime.gameObject.SetActive(true);
        float coolTime = (_availShotAt - PingManager.GameTime) / 1000.0f;
        float fullCoolTime = coolTime;
        while (coolTime >= 0)
        {
            coolTime -= Time.deltaTime;

            _shotCoolTime.text = string.Format("{0}", (int)coolTime);
            _shotImg.fillAmount = 1.0f - (coolTime / fullCoolTime);

            yield return new WaitForFixedUpdate();
        }
        ShotInit();
    }
    public void ShotInit()
    {
        _shotImg.fillAmount = 1;
        _shotCoolTime.gameObject.SetActive(false);
    }
    public void Shot()
    {
        if (_shotTargetUserType == roomUserType.USER.ToString())
        {
            if (_shotTargetCrimeType == crimeType.SPY.ToString())
            {
                StartCoroutine("CriminalShotMotion", _shotSpyCamera);
            }
            else if (_shotTargetCrimeType == crimeType.BOOMER.ToString())
            {
                StartCoroutine("CriminalShotMotion", _shotBoomerCamera);
            }
            else if (_shotTargetCrimeType == crimeType.ASSASSIN.ToString())
            {
                StartCoroutine("CriminalShotMotion", _shotAssassinCamera);
            }
        }
        else
        {
            StartCoroutine("NpcShotMotion");
            StartCoroutine("ShotResult", "무고한 시민을 체포했습니다...");
        }
    }
    private void ToggleUI()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (_jobCanvas.activeSelf)
                _jobCanvas.SetActive(false);
            else
                _jobCanvas.SetActive(true);
        }
    }
    /// <summary>
    /// 알림용 메시지 출력
    /// </summary>
    /// <param name="msg">출력하고 싶은 메시지 / ""으로 전달시 오브젝트 제거</param>
    private void AlertMsg(string msg)
    {
        _alert.SetActive(msg != "");
        _alert.GetComponentInChildren<TextMeshProUGUI>().text = msg;
    }
    private void ReportMsg(string msg)
    {
        _reportAlert.SetActive(msg != "");
        _reportAlert.GetComponentInChildren<TextMeshProUGUI>().text = msg;
    }
    /// <summary>
    /// 재접속 데이터
    /// </summary>
    private void PlayerReplace()
    {
        if (ModelManager._isReconnected)
        {
            //위치, 방향 업데이트
            Vector3 pos = new Vector3(RoomInfo._roomCop.pos.x, 0, RoomInfo._roomCop.pos.z);
            Quaternion rot = Quaternion.Euler(RoomInfo._roomCop.rot.x, RoomInfo._roomCop.rot.y, RoomInfo._roomCop.rot.z);
            GetComponent<Transform>().position = pos;
            GetComponent<Transform>().rotation = rot;

            //신고 메시지 띄우기
            if (ReconnectionController._recentReportForCop.Length != 0)
            {
                foreach (ReportInfo report in ReconnectionController._recentReportForCop)
                {
                    _reportUsername = report.reportUsername;
                    _reportTargetUsername = report.targetUsername;
                    _highlightStartAt = Convert.ToInt64(report.highlightStartAt);
                    _highlightEndAt = Convert.ToInt64(report.highlightEndAt);
                    if (ModelManager._users[_reportTargetUsername].crimeType == crimeType.SPY.ToString())
                    {
                        StartCoroutine(ReportResult(1, _highlightStartAt, _highlightEndAt));
                    }
                    if (ModelManager._users[_reportTargetUsername].crimeType == crimeType.BOOMER.ToString())
                    {
                        StartCoroutine(ReportResult(2, _highlightStartAt, _highlightEndAt));
                    }
                    if (ModelManager._users[_reportTargetUsername].crimeType == crimeType.ASSASSIN.ToString())
                    {
                        StartCoroutine(ReportResult(3, _highlightStartAt, _highlightEndAt));
                    }
                    Debug.Log("신고 발생");
                }
                ReconnectionController._recentReportForCop = new ReportInfo[0];
            }
        }
    }
    IEnumerator ReportResult(int criminalNum, long highlightStartAt, long highlightEndAt)
    {
        float reportStartTime = (highlightStartAt - PingManager.GameTime) / 1000.0f;
        float reportEndTime = (highlightEndAt - PingManager.GameTime) / 1000.0f;
        bool pingOnce = true;
        rVector3 eventPos = new rVector3(0, 0, 0);
        switch (criminalNum)
        {
            case 1:
                eventPos = RoomInfo._roomSpy.pos;
                break;
            case 2:
                eventPos = RoomInfo._roomBoomer.pos;
                break;
            case 3:
                eventPos = RoomInfo._roomAssassin.pos;
                break;
            default:
                break;
        }

        while (reportEndTime >= 0)
        {
            reportEndTime -= Time.deltaTime;
            reportStartTime -= Time.deltaTime;
            if (reportStartTime < 0)
            {
                ReportMsg("신고가 발생하였습니다!");
                RoomInfo._reportTarget[criminalNum] = true;
                if (pingOnce)
                {
                    _minimap.GetComponent<MinimapController>().MapPingReport(new Vector3(eventPos.x, eventPos.y, eventPos.z), true);
                    pingOnce = false;
                }
            }
            yield return new WaitForFixedUpdate();
        }
        RoomInfo._reportTarget[criminalNum] = false;
        _minimap.GetComponent<MinimapController>().MapPingReport(new Vector3(eventPos.x, eventPos.y, eventPos.z), false);
        ReportMsg("");
    }
    IEnumerator ShotResult(string msg)
    {
        //1 - 범죄자 사격시 / 2 - 시민 사격시
        float msgPrintTime = 3f;
        AlertMsg(msg);
        while (msgPrintTime >= 0)
        {
            msgPrintTime -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        AlertMsg("");
    }
    IEnumerator CriminalShotMotion(GameObject shotTargetCamera)
    {
        float motionTime = 0f;
        bool shotOnce = true;
        _shotCamera.Priority = 13;
        while (motionTime < 3)
        {
            motionTime += Time.deltaTime;
            float x = Mathf.Lerp(-2000, -555, motionTime);
            shotTargetCamera.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, 0, 0);
            if (shotOnce && motionTime > 2.5)
            {
                _gun.GetComponent<Animator>().SetTrigger("Shot");
                shotOnce = false;
            }
            yield return new WaitForFixedUpdate();
        }
        EndStun();
        _shotCamera.Priority = 10;
        shotTargetCamera.SetActive(false);
        StartCoroutine("ShotResult", "성공적으로 범죄자를 체포하였습니다!");
    }
    IEnumerator NpcShotMotion()
    {
        _gun.GetComponent<Animator>().SetTrigger("Shot");
        yield return new WaitForSeconds(1f);
        EndStun();
    }
    //PACKET DATA
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
                color = 1,
                roomUserType = ModelManager._userType,
                userState = userState.NORMAL.ToString(),
                crimeType = ModelManager._crimeType
            };
            this.roomId = ModelManager._roomId;
        }
    }
    //Updated 함수
    private void StunUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        StunResponse StunInfo = StunResponse.CreateFromJSON(e.Data);
        if (StunInfo.stunState == stunState.START.ToString())
        {
            ModelManager._whileStun = true;
            _availShotAt = Convert.ToInt64(StunInfo.availShotAt);
            _justStun = true;
        }
        if (StunInfo.stunState == stunState.END.ToString())
        {
            ModelManager._whileStun = false;
            _releaseStun = true;
            //사격한경우 스턴해제시 쿨타임 적용 안함
            if (!_afterShot)
            {
                _nextStunAvailAt = Convert.ToInt64(StunInfo.nextStunAvailAt);
            }
        }
    }
    private void ShotUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        ShotResponse ShotInfo = ShotResponse.CreateFromJSON(e.Data);

        _hasShot = true;
        _afterShot = true;
        _shotTarget = ShotInfo.targetUsername;
        _shotTargetUserType = ShotInfo.targetUserType;
        _shotTargetCrimeType = ShotInfo.targetUserCrimeType;
        _aliveUserCount = ShotInfo.aliveUserCount;
        _nextStunAvailAt = Convert.ToInt64(ShotInfo.stunAvailAt);
    }
    private void MissionUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        PlayMissionResponse MissionInfo = PlayMissionResponse.CreateFromJSON(e.Data);
        if (MissionInfo.missionState == missionState.CLEAR.ToString())
        {
            _missionSuccess = true;
            if (MissionInfo.crimeType == crimeType.SPY.ToString())
            {
                RoomInfo._finishedMission = 1;
            }
            if (MissionInfo.crimeType == crimeType.BOOMER.ToString())
            {
                RoomInfo._finishedMission = 2;
            }
            if (MissionInfo.crimeType == crimeType.ASSASSIN.ToString())
            {
                RoomInfo._finishedMission = 3;
            }
        }
        else if (MissionInfo.missionState == missionState.FAIL.ToString())
        {
            _missionFailed = true;
            if (MissionInfo.crimeType == crimeType.SPY.ToString())
            {
                RoomInfo._finishedMission = 1;
            }
            if (MissionInfo.crimeType == crimeType.BOOMER.ToString())
            {
                RoomInfo._finishedMission = 2;
            }
            if (MissionInfo.crimeType == crimeType.ASSASSIN.ToString())
            {
                RoomInfo._finishedMission = 3;
            }
        }
    }
    private void ReportUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        ReportUserResponse reportInfo = ReportUserResponse.CreateFromJSON(e.Data);
        _justReport = true;
        _reportUsername = reportInfo.reportUsername;
        _reportTargetUsername = reportInfo.targetUsername;
        _highlightStartAt = Convert.ToInt64(reportInfo.highlightStartAt);
        _highlightEndAt = Convert.ToInt64(reportInfo.highlightEndAt);
    }
    private void KilledUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        AssassinKillResponse KilledInfo = AssassinKillResponse.CreateFromJSON(e.Data);
    }
    //Request 전송
    public void sendRequestAim(string aimState)
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        RequestAim(aimState, networking);
    }
    //Stun Req
    public void sendRequestStun(string username, string stunState)
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        RequestStun(username, stunState, networking);
    }
    //Shot Req
    public void sendRequestShot()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        RequestShot(networking);
    }
    private void RequestAim(string aimState, NetworkingController networking)
    {
        var request = new AimRequest(_aimPos, PingManager.GameTime.ToString(), aimState);
        networking.sendRequest(request);
    }
    private void RequestStun(string username, string stunState, NetworkingController networking)
    {
        //검문시 타겟 유저네임이랑 시간 보내줌
        var request = new StunRequest(username, PingManager.GameTime.ToString(), stunState);
        networking.sendRequest(request);
    }
    private void RequestShot(NetworkingController networking)
    {
        var request = new ShotRequest(PingManager.GameTime.ToString());
        networking.sendRequest(request);
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
        public string stunAvailAt;
        public string targetUserCrimeType;
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
    public class AimRequest : ClientRequest
    {
        public rVector3 aimPos;
        public string aimAt;
        public Int32 roomId;
        public string aimState;
        public AimRequest(rVector3 aimPos, string aimAt, string aimState)
        {
            type = requestType.AIM.ToString();
            this.aimPos = aimPos;
            this.aimAt = aimAt;
            this.aimState = aimState;
            roomId = ModelManager._roomId;
        }
    }
    public class StunRequest : ClientRequest
    {
        public string targetUsername;
        public string targetingAt;
        public string stunState;
        public Int32 roomId;
        public StunRequest(string target, string targetAt, string stunState)
        {
            type = requestType.STUN.ToString();
            this.targetUsername = target;
            this.targetingAt = targetAt;
            this.stunState = stunState;
            roomId = ModelManager._roomId;
        }
    }
    public class ShotRequest : ClientRequest
    {
        public string shotAt;
        public Int32 roomId;
        public ShotRequest(string shotAt)
        {
            type = requestType.SHOT.ToString();
            this.shotAt = shotAt;
            roomId = ModelManager._roomId;
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