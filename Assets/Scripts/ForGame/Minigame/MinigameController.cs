using System;
using Cinemachine;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using TMPro;
using System.Collections;
using Unity.VisualScripting;
using System.Linq;
using System.Runtime.InteropServices;

public class MinigameController : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [Header("Mission")]
    [SerializeField] private TextMeshProUGUI _currentMission;
    [SerializeField] private GameObject _missionLight;
    [SerializeField] private GameObject _missionMsg;
    [SerializeField] private GameObject _plantMsg;
    [SerializeField] private GameObject _missionResult;
    [SerializeField] private GameObject _commonMinigame;
    [SerializeField] private GameObject _spySpecialMinigame;
    [SerializeField] private GameObject _boomerSpecialMinigame;
    [SerializeField] private GameObject _canvas;
    [SerializeField] private GameObject _minimap;
    //Mission
    private bool _whileMission = false;
    private bool _isMissionClear = false;
    private bool _justFinishedMission = false;
    private GameObject _missionLocation;
    private GameObject _commonMissionCanvas;
    private GameObject _spyMissionCanvas;
    private GameObject _BoomerMissionCanvas;
    private rVector3 _nextMissionLocation;
    private bool _failedMission = false;
    private int _totalMission;
    private bool _missionValid = true;
    private bool _pingOnce = true;
    //Assassin
    private Ray _ray;
    private RaycastHit _targetHit;
    private GameObject _targetObject;
    private Transform _focused;
    [SerializeField] private GameObject _takeDownMsg;
    private static readonly string _missionResponseName = responseType.PLAY_MISSION.ToString();
    void Awake()
    {
        gameObject.SetActive(ModelManager._userType != roomUserType.COP.ToString());
    }
    void Start()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.startListenOnMessage(MissionUpdated, _missionResponseName);
        //미션 빛기둥 활성화
        if (!(ModelManager._crimeType == crimeType.ASSASSIN.ToString() && ModelManager._missionPhase == 3))
        {
            _nextMissionLocation = ModelManager._missionLocation[ModelManager._missionPhase];
            _missionLocation = Instantiate(_missionLight, new Vector3(_nextMissionLocation.x, 0, _nextMissionLocation.z), Quaternion.identity);
        }
        if (ModelManager._crimeType == crimeType.ASSASSIN.ToString())
            _totalMission = 3;
        else if (ModelManager._crimeType == crimeType.SPY.ToString())
            _totalMission = 4;
        else if (ModelManager._crimeType == crimeType.BOOMER.ToString())
            _totalMission = 4;
    }
    void OnDestroy()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.stopListenOnMessage(MissionUpdated, _missionResponseName);
    }
    void Update()
    {
        if (!ModelManager._isDisconnected)
        {
            if (!ModelManager._isDead)
            {
                if (ModelManager._crimeType == crimeType.SPY.ToString())
                {
                    SpyMission();
                }
                if (ModelManager._crimeType == crimeType.BOOMER.ToString())
                {
                    BoomerMission();
                }
                if (ModelManager._crimeType == crimeType.ASSASSIN.ToString())
                {
                    AssassinMission();
                }
                _currentMission.text = (ModelManager._missionPhase - 1).ToString() + " / " + _totalMission;
                if (!(ModelManager._crimeType == crimeType.ASSASSIN.ToString() && ModelManager._missionPhase == 3))
                    _missionLocation.transform.position = new Vector3(_nextMissionLocation.x, _nextMissionLocation.y, _nextMissionLocation.z);
                if (_justFinishedMission)
                {
                    if (_isMissionClear)
                    {
                        _minimap.GetComponent<MinimapController>().MapPing(new Vector3(_nextMissionLocation.x, _nextMissionLocation.y, _nextMissionLocation.z), false);
                    }
                    _justFinishedMission = false;
                    StartCoroutine("PrintMissionResult", _isMissionClear);
                }
            }
            else
            {
                Destroy(_BoomerMissionCanvas);
                Destroy(_spyMissionCanvas);
                Destroy(_commonMissionCanvas);
                gameObject.SetActive(false);
            }
        }
    }
    /// <summary>
    /// 스파이 미션 로직
    /// </summary>
    private void SpyMission()
    {
        //미니맵에 핑찍기
        if (_pingOnce)
        {
            _minimap.GetComponent<MinimapController>().MapPing(new Vector3(_nextMissionLocation.x, _nextMissionLocation.y, _nextMissionLocation.z), true);
            _pingOnce = false;
        }
        if (_missionValid)
        {
            if (!ModelManager._isStunned && !_whileMission)
            {
                //1,2,3 번째 미션
                if (ModelManager._missionPhase < _totalMission)
                {
                    if (!_failedMission)
                    {
                        //미션장소와 가까울때 진행
                        if (Vector3.Distance(_player.transform.position, new Vector3(_nextMissionLocation.x, _nextMissionLocation.y, _nextMissionLocation.z)) < 1)
                        {
                            _missionMsg.SetActive(true);
                            if (Input.GetKeyDown(KeyCode.E) && !ModelManager._whileChat)
                            {
                                sendRequestMission(_nextMissionLocation, missionState.START.ToString());
                                _commonMissionCanvas = Instantiate(_commonMinigame);
                                _commonMissionCanvas.transform.parent = _canvas.transform;
                            }
                        }
                        else
                            _missionMsg.SetActive(false);
                    }
                }
                //최종 미션
                else if (ModelManager._missionPhase == _totalMission)
                {
                    if (_focused != null)
                    {
                        if (_targetObject != null)
                        {
                            _targetObject.GetComponent<Outline>().enabled = false;
                            _missionMsg.SetActive(false);
                        }
                        _focused = null;
                    }
                    _ray = new Ray(_player.transform.position + _player.transform.forward / 3 + _player.transform.up, _player.transform.forward);
                    if (Physics.Raycast(_ray, out _targetHit, 0.5f))
                    {
                        _targetObject = _targetHit.transform.gameObject;
                        //상대가 해킹장소면
                        if (_targetObject.layer == LayerMask.NameToLayer("Hack"))
                        {
                            _targetObject.GetComponent<Outline>().enabled = true;
                            _missionMsg.SetActive(true);
                            if (Input.GetKeyDown(KeyCode.E))
                            {
                                sendRequestMission(_nextMissionLocation, missionState.START.ToString());
                                //스파이 미니게임 시작
                                _spyMissionCanvas = Instantiate(_spySpecialMinigame);
                                _spyMissionCanvas.transform.SetParent(_canvas.transform);
                            }
                            _focused = _targetHit.transform;
                        }
                    }
                }
            }
        }
        else
        {
            _missionMsg.SetActive(false);
        }
    }
    private void BoomerMission()
    {
        if (_pingOnce)
        {
            _minimap.GetComponent<MinimapController>().MapPing(new Vector3(_nextMissionLocation.x, _nextMissionLocation.y, _nextMissionLocation.z), true);
            _pingOnce = false;
        }
        if (_missionValid)
        {
            if (!ModelManager._isStunned && !_whileMission)
            {
                //첫번째 미션
                if (ModelManager._missionPhase == 1)
                {
                    if (_focused != null)
                    {
                        if (_targetObject != null)
                        {
                            //_targetObject.GetComponent<MeshRenderer>().enabled = false;
                            _plantMsg.SetActive(false);
                        }
                        _focused = null;
                    }
                    _ray = new Ray(_player.transform.position + _player.transform.forward / 3 + _player.transform.up, _player.transform.forward);
                    if (Physics.Raycast(_ray, out _targetHit, 0.5f))
                    {
                        _targetObject = _targetHit.transform.gameObject;
                        //상대가 설치장소면
                        if (_targetObject.layer == LayerMask.NameToLayer("Plant"))
                        {
                            _targetObject.GetComponent<MeshRenderer>().enabled = true;
                            _plantMsg.SetActive(true);
                            if (Input.GetKeyDown(KeyCode.E))
                            {
                                sendRequestMission(_nextMissionLocation, missionState.START.ToString());
                                //폭탄 설치 시스템 시작
                                _BoomerMissionCanvas = Instantiate(_boomerSpecialMinigame, _canvas.transform);
                            }
                            _focused = _targetHit.transform;
                        }
                    }

                }
                //2,3,4번째 미션
                else if (ModelManager._missionPhase <= _totalMission)
                {
                    _plantMsg.SetActive(false);
                    //미션장소와 가까울때 진행
                    if (Vector3.Distance(_player.transform.position, new Vector3(_nextMissionLocation.x, _nextMissionLocation.y, _nextMissionLocation.z)) < 1)
                    {
                        _missionMsg.SetActive(true);
                        if (Input.GetKeyDown(KeyCode.E) && !ModelManager._whileChat)
                        {
                            sendRequestMission(_nextMissionLocation, missionState.START.ToString());
                            _commonMissionCanvas = Instantiate(_commonMinigame);
                            _commonMissionCanvas.transform.SetParent(_canvas.transform);
                        }
                    }
                    else
                        _missionMsg.SetActive(false);
                }
            }
        }
        else
        {
            _plantMsg.SetActive(false);
            _missionMsg.SetActive(false);
        }
        if (ModelManager._missionPhase > _totalMission)
        {
            _missionLocation.SetActive(false);
        }
    }
    private void AssassinMission()
    {
        if (ModelManager._missionPhase < _totalMission)
        {
            if (_missionValid)
            {
                _missionMsg.SetActive(Vector3.Distance(_player.transform.position, new Vector3(_nextMissionLocation.x, _nextMissionLocation.y, _nextMissionLocation.z)) < 1);
            }
            else
            {
                _missionMsg.SetActive(false);
            }
            if (_pingOnce)
            {
                _minimap.GetComponent<MinimapController>().MapPing(new Vector3(_nextMissionLocation.x, _nextMissionLocation.y, _nextMissionLocation.z), true);
                _pingOnce = false;
            }
        }
        if (!ModelManager._isStunned && !_whileMission)
        {
            //1,2번째 미션
            if (ModelManager._missionPhase < _totalMission)
            {
                if (!_failedMission)
                {
                    //미션장소와 가까울때 진행
                    if (Vector3.Distance(_player.transform.position, new Vector3(_nextMissionLocation.x, _nextMissionLocation.y, _nextMissionLocation.z)) < 1)
                    {
                        if (Input.GetKeyDown(KeyCode.E) && !ModelManager._whileChat)
                        {
                            sendRequestMission(_nextMissionLocation, missionState.START.ToString());
                            _commonMissionCanvas = Instantiate(_commonMinigame);
                            _commonMissionCanvas.transform.parent = _canvas.transform;
                        }
                    }
                }
            }
            //최종미션 시작
            else if (ModelManager._missionPhase == _totalMission)
            {
                if (_missionLocation != null)
                    _missionLocation.SetActive(false);
                //_missionMsg.SetActive(false);
                //sendRequestMission(_nextMissionLocation, missionState.START.ToString());
                //타겟 표시 및 암살 시스템 시작
                AssasssinSpecialMission();
            }
        }
    }
    private void AssasssinSpecialMission()
    {
        if (_focused != null)
        {
            if (_targetObject != null)
            {
                _targetObject.GetComponent<Outline>().OutlineColor = Color.red;
                _takeDownMsg.SetActive(false);
            }
            _focused = null;
        }
        _ray = new Ray(_player.transform.position + _player.transform.forward / 3 + _player.transform.up, _player.transform.forward);
        if (Physics.Raycast(_ray, out _targetHit, 1f))
        {
            _targetObject = _targetHit.transform.parent.gameObject;
            //상대가 캐릭터면
            if (_targetObject.layer == LayerMask.NameToLayer("Character"))
            {
                //상대 캐릭터가 타겟이면
                string targetName = _targetObject.GetComponent<PlayerInfo>()._roomUser.username;
                if (ModelManager._targetID.Contains(targetName))
                {
                    _targetObject.GetComponent<Outline>().OutlineColor = Color.green;
                    _takeDownMsg.SetActive(true);
                    if (Input.GetKeyDown(KeyCode.E) && !ModelManager._whileChat)
                    {
                        sendRequestAssassinKill(targetName);
                    }
                    _focused = _targetHit.transform;
                }
            }
        }
    }
    IEnumerator PrintMissionResult(bool result)
    {
        float msgPrintTime = 3f;
        MissionMsg(result ? "임무 성공!" : "임무 실패...");
        while (msgPrintTime >= 0)
        {
            msgPrintTime -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        _failedMission = false;
        _missionValid = true;
        MissionMsg("");
    }
    private void MissionMsg(string msg)
    {
        _missionResult.SetActive(msg != "");
        _missionResult.GetComponentInChildren<TextMeshProUGUI>().text = msg;
    }
    private void MissionUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        PlayMissionResponse MissionInfo = PlayMissionResponse.CreateFromJSON(e.Data);
        if (MissionInfo.username == ModelManager._username)
        {
            if (MissionInfo.missionState == missionState.START.ToString())
            {
                _whileMission = true;
                _missionValid = false;
            }
            //미션 성공시
            else if (MissionInfo.missionState == missionState.CLEAR.ToString())
            {
                ModelManager._missionPhase++;
                _pingOnce = true;
                if (ModelManager._missionPhase <= ModelManager._missionLocation.Count)
                    _nextMissionLocation = ModelManager._missionLocation[ModelManager._missionPhase];
                _isMissionClear = true;
                _whileMission = false;
                _justFinishedMission = true;
            }
            //미션 실패시
            else if (MissionInfo.missionState == missionState.FAIL.ToString())
            {
                _isMissionClear = false;
                _whileMission = false;
                _justFinishedMission = true;
                _failedMission = true;
            }
        }
    }
    public void sendRequestMission(rVector3 missionPos, string missionState)
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        RequestPlayMission(networking, missionPos, missionState);
    }
    private void RequestPlayMission(NetworkingController networking, rVector3 missionPos, string missionState)
    {
        var request = new PlayMissionReqeust(ModelManager._missionPhase, missionPos, missionState);
        networking.sendRequest(request);
    }
    public void sendRequestAssassinKill(string target)
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        RequestAssassinKill(networking, target);
    }
    private void RequestAssassinKill(NetworkingController networking, string target)
    {
        var request = new AssassinKillRequest(target);
        networking.sendRequest(request);
    }
    public class AssassinKillRequest : ClientRequest
    {
        public string targetUsername;
        public int roomId;
        public AssassinKillRequest(string targetUsername)
        {
            type = requestType.ASSASSIN_KILL.ToString();
            this.targetUsername = targetUsername;
            this.roomId = ModelManager._roomId;
        }
    }
    public class PlayMissionReqeust : ClientRequest
    {
        public int roomId;
        public int missionPhase;
        public rVector3 missionPos;
        public string crimeType;
        public string username;
        public string missionState;
        public PlayMissionReqeust(int missionPhase, rVector3 missionPos, string missionState)
        {
            type = requestType.PLAY_MISSION.ToString();
            this.roomId = ModelManager._roomId;
            this.missionPhase = missionPhase;
            this.missionPos = missionPos;
            this.crimeType = ModelManager._crimeType;
            this.username = ModelManager._username;
            this.missionState = missionState;
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
}