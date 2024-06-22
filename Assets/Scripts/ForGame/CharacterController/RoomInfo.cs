using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using TMPro;
using System.Collections;

public class RoomInfo : MonoBehaviour
{
    public static RoomUser _roomCop;
    public static RoomUser _roomSpy;
    public static RoomUser _roomBoomer;
    public static RoomUser _roomAssassin;
    public static List<RoomUser> _roomNPCs = new List<RoomUser>();

    [SerializeField] private GameObject _bomb;
    [SerializeField] private GameObject _realBomb;
    [SerializeField] private Material _bombMaterial;
    [SerializeField] private TextMeshProUGUI _timer;
    [SerializeField] private TextMeshProUGUI _bombTimer;
    [SerializeField] private GameObject _plantMsg;
    [SerializeField] private GameObject _decreaseMsg;
    // 0 : 없음 / 1 : spy / 2 : boomer / 3 : assassin
    public static int _finishedMission = 0;
    public static bool[] _reportTarget = { false, false, false, false, false };
    //시작위치 data
    private DateTime _gameTime;
    private DateTime _boomTime;
    private long _boomAt = 0;
    private long _prevBoomAt = 0;
    private bool _printOnce = true;
    private rVector3 _copStartPos;
    private rVector3 _copStartRot;
    private List<rVector3> _npcStartPos;
    private List<rVector3> _npcStartRot;
    private rVector3 _spyStartPos;
    private rVector3 _spyStartRot;
    private rVector3 _boomerStartPos;
    private rVector3 _boomerStartRot;
    private rVector3 _assassinStartPos;
    private rVector3 _assassinStartRot;
    private static readonly String _responseName = responseType.PLAY_ROOM_INFO.ToString();
    void Start()
    {
        //마우스 화면 중앙고정
        Cursor.lockState = CursorLockMode.Locked;

        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.startListenOnMessage(UserUpdated, _responseName);

        ModelManager._whileGame = true;

        _copStartPos = ModelManager._copPosition;
        _copStartRot = ModelManager._copRotation;
        _npcStartPos = ModelManager._npcPosition;
        _npcStartRot = ModelManager._npcRotation;
        _spyStartPos = ModelManager._spyPosition;
        _spyStartRot = ModelManager._spyRotation;
        _boomerStartPos = ModelManager._boomerPosition;
        _boomerStartRot = ModelManager._boomerRotation;
        _assassinStartPos = ModelManager._assassinPosition;
        _assassinStartRot = ModelManager._assassinRotation;
        //초기 위치 저장
        _roomCop = new RoomUser() { pos = _copStartPos, rot = _copStartRot };
        _roomSpy = new RoomUser() { pos = _spyStartPos, rot = _spyStartRot };
        _roomBoomer = new RoomUser() { pos = _boomerStartPos, rot = _boomerStartRot };
        _roomAssassin = new RoomUser() { pos = _assassinStartPos, rot = _assassinStartRot };

        _decreaseMsg.GetComponent<TextMeshProUGUI>().text = "-" + ModelManager._boomerMinigame.reducedTimePerMissionClear.ToString() + ":00";

        for (int i = 0; i < ModelManager._npcCount; i++)
        {
            RoomUser roomUser = new RoomUser() { pos = _npcStartPos[i], rot = _npcStartRot[i], roomUserType = roomUserType.NPC.ToString() };
            _roomNPCs.Add(roomUser);
        }
        if (ModelManager._crimeType == crimeType.BOOMER.ToString())
        {
            _realBomb.SetActive(true);
        }
        _reportTarget[1] = false;
        _reportTarget[2] = false;
        _reportTarget[3] = false;
        _reportTarget[4] = false;
    }
    void Update()
    {
        updateTime();
        if (ModelManager._whileChat)
        {
            //Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    void updateTime()
    {
        _gameTime = DateTimeOffset.FromUnixTimeSeconds((ModelManager._endTime - PingManager.GameTime) / 1000).UtcDateTime;
        _timer.text = _gameTime.ToString("mm : ss");
        if (_boomAt != 0)
        {
            _bomb.SetActive(true);
            _boomTime = DateTimeOffset.FromUnixTimeSeconds((_boomAt - PingManager.GameTime) / 1000).UtcDateTime;
            _bombTimer.text = _boomTime.ToString("mm:ss");
            //폭파시간 바꼈을때
            if (_prevBoomAt != _boomAt && !_printOnce)
            {
                StartCoroutine("TimerDecrease");
            }
            if (_printOnce)
            {
                _realBomb.SetActive(true);
                _realBomb.GetComponent<MeshRenderer>().material = _bombMaterial;
                StartCoroutine("BombPlanted");
                _printOnce = false;
            }
            //1분보다 적게남았을때
            if (_boomAt - PingManager.GameTime < 60000)
            {
                _bombTimer.color = Color.red;
            }
            _prevBoomAt = _boomAt;
        }
    }
    void OnDestroy()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.stopListenOnMessage(UserUpdated, _responseName);
    }
    IEnumerator TimerDecrease()
    {
        _decreaseMsg.SetActive(true);
        yield return new WaitForSeconds(3);
        _decreaseMsg.SetActive(false);
    }
    IEnumerator BombPlanted()
    {
        _plantMsg.SetActive(true);
        yield return new WaitForSeconds(4);
        _plantMsg.SetActive(false);
    }
    private void UserUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        PlayRoomInfoResponse roomInfo = PlayRoomInfoResponse.CreateFromJSON(e.Data);
        _boomAt = Convert.ToInt64(roomInfo.boomAt);
        int npcCount = 0;
        for (int i = 0; i < roomInfo.roomUsers.Length; i++)
        {
            //username 할당해주는 단계는 후에 수정해서 로비에서 settingroom으로 되도록 수정!'
            //NPC정보만 settingRoom에서 할당중
            if (roomInfo.roomUsers[i].roomUserType == roomUserType.COP.ToString())
            {
                _roomCop = roomInfo.roomUsers[i];
            }
            if (roomInfo.roomUsers[i].roomUserType == roomUserType.USER.ToString())
            {
                if (roomInfo.roomUsers[i].crimeType == crimeType.SPY.ToString())
                {
                    _roomSpy = roomInfo.roomUsers[i];
                }
                else if (roomInfo.roomUsers[i].crimeType == crimeType.BOOMER.ToString())
                {
                    _roomBoomer = roomInfo.roomUsers[i];
                }
                else if (roomInfo.roomUsers[i].crimeType == crimeType.ASSASSIN.ToString())
                {
                    _roomAssassin = roomInfo.roomUsers[i];
                }
            }
            if (roomInfo.roomUsers[i].roomUserType == roomUserType.NPC.ToString())
            {
                _roomNPCs[npcCount] = roomInfo.roomUsers[i];
                npcCount++;
            }
            /*연결 끊긴후에 스턴해제되면 자동으로 해제
            if (roomInfo.roomUsers[i].username == ModelManager._username && ModelManager._isReconnected)
            {
                if (roomInfo.roomUsers[i].userState == userState.NORMAL.ToString())
                    ModelManager._isStunned = false;
            }
            */
        }
    }
    public class PlayRoomInfoResponse
    {
        public RoomUser[] roomUsers;
        public string roomEndAt;
        public string boomAt;
        public static PlayRoomInfoResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<PlayRoomInfoResponse>(jsonString);
        }
    }
}