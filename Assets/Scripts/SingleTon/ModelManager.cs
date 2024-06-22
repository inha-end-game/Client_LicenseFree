using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using client.Assets.Scripts.Class;
public class ModelManager : MonoBehaviour
{
    private static ModelManager _instance;
    //Data Model 객체
    [Tooltip("movement")]
    public static Movement _criminalMovement = new Movement();
    public static Movement _copMovement = new Movement();
    public static Movement _npcMovement = new Movement();
    [Tooltip("map")]
    public static Map _map = new Map();
    [Tooltip("spawn")]
    public static Spawn _copSpawn = new Spawn();
    public static Spawn _npcInnerSpawn = new Spawn();
    public static Spawn _npcOuterSpawn = new Spawn();
    public static Spawn _criminalInnerSpawn = new Spawn();
    public static Spawn _criminalOuterSpawn = new Spawn();
    [Tooltip("EventScreen")]
    public static EventScreen _screen = new EventScreen();
    [Tooltip("motion")]
    public static Motion _surpriseMotion = new Motion();
    public static Motion _fearMotion = new Motion();
    public static Motion _danceMotion = new Motion();
    public static Motion _clapMotion = new Motion();
    public static Motion _stopMotion = new Motion();
    public static Motion _interactMotion = new Motion();
    public static Motion _inspectMotion = new Motion();
    public static Motion _shootMotion = new Motion();
    public static Motion _reportMotion = new Motion();
    [Tooltip("shot")]
    public static Shot _shotRule = new Shot();
    [Tooltip("spyMinigame")]
    public static spyMinigame _spyMinigame = new spyMinigame();
    [Tooltip("criminalMinigame")]
    public static criminalMinigame _criminalMinigame = new criminalMinigame();
    [Tooltip("assassinMinigame")]
    public static assassinMinigame _assassinMinigame = new assassinMinigame();
    [Tooltip("boomerMinigame")]
    public static boomerMinigame _boomerMinigame = new boomerMinigame();
    [Tooltip("report")]
    public static report _report = new report();
    //게임 상태
    public static string _ip;
    public static bool _isDisconnectedInGame = false;
    public static bool _isDisconnectedCloseGame = false;
    public static bool _isDisconnectedInLobby = false;
    public static bool _isDisconnected = false;
    public static bool _isReconnected = false;
    public static bool _checkOnce = true;
    public static bool _endOnce = true;
    //방정보
    public static bool _isRegister = false;
    public static int _roomId = 0;
    public static long _endTime;
    public static bool _whileGame = false;
    public static bool _whileConnected = false;
    public static bool _updateUserList = false;
    public static RoomUser[] _roomUserList = null;
    //게임 종료 정보
    public static GameOverInfo _gameOverInfo;
    //남은 범죄자 수
    public static int _leftCriminal = 0;
    //아이템 
    public static int _itemValid;
    //미션
    public static Dictionary<int, rVector3> _missionLocation = new Dictionary<int, rVector3>();
    public static List<string> _targetID = new List<string>();
    public static int _missionPhase;
    public static bool _whileMission = false;
    public static string[] _spyCode = { "NPC", "SPY", "COP" };
    //검문, 사격
    public static bool _whileStun = false;
    public static bool _isStunned = false;
    public static bool _hasJustStunned = false;
    public static bool _isDead = false;
    //초기 위치 저장하는 LIST
    public static rVector3 _copPosition = new rVector3(50, 0, 50);
    public static rVector3 _copRotation = new rVector3(0, 0, 0);
    public static long _npcCount = 100;
    public static List<rVector3> _npcPosition = new List<rVector3>();
    public static List<rVector3> _npcRotation = new List<rVector3>();
    public static List<int> _npcColor = new List<int>();
    public static rVector3 _spyPosition = new rVector3(-50, 0, -50);
    public static rVector3 _spyRotation = new rVector3(0, 0, 0);
    public static int _spyColor;
    public static rVector3 _boomerPosition = new rVector3(-50, 0, -50);
    public static rVector3 _boomerRotation = new rVector3(0, 0, 0);
    public static int _boomerColor;
    public static rVector3 _assassinPosition = new rVector3(-50, 0, -50);
    public static rVector3 _assassinRotation = new rVector3(0, 0, 0);
    public static int _assassinColor;
    //사용자 정보
    public static Dictionary<string, RoomUser> _users = new Dictionary<string, RoomUser>();
    public static string _copNickname;
    public static string _spyNickname;
    public static string _boomerNickname;
    public static string _assasssinNickname;

    // 내정보 전달
    public static int _userColor;
    public static string _username;
    public static string _nickname;
    public static string _userType;
    public static string _crimeType;
    public static bool _whileChat = false;
    void Start()
    {
        _criminalMovement = ToModel<Movement>(JsonReader.GetModel("movement", "stat_user_criminal"));
        _copMovement = ToModel<Movement>(JsonReader.GetModel("movement", "stat_user_cop"));
        _npcMovement = ToModel<Movement>(JsonReader.GetModel("movement", "stat_npc"));

        _map = ToModel<Map>(JsonReader.GetModel("map", "map_size"));

        _copSpawn = ToModel<Spawn>(JsonReader.GetModel("spawn", "spawn_cop"));
        _npcInnerSpawn = ToModel<Spawn>(JsonReader.GetModel("spawn", "spawn_npc_inner"));
        _npcOuterSpawn = ToModel<Spawn>(JsonReader.GetModel("spawn", "spawn_npc_outer"));
        _criminalInnerSpawn = ToModel<Spawn>(JsonReader.GetModel("spawn", "spawn_criminal_inner"));
        _criminalOuterSpawn = ToModel<Spawn>(JsonReader.GetModel("spawn", "spawn_criminal_outer"));

        _screen = ToModel<EventScreen>(JsonReader.GetModel("screen", "screen"));

        _surpriseMotion = ToModel<Motion>(JsonReader.GetModel("motion", "motion_surprised"));
        _fearMotion = ToModel<Motion>(JsonReader.GetModel("motion", "motion_fear"));
        _danceMotion = ToModel<Motion>(JsonReader.GetModel("motion", "motion_dance"));
        _clapMotion = ToModel<Motion>(JsonReader.GetModel("motion", "motion_clap"));
        _stopMotion = ToModel<Motion>(JsonReader.GetModel("motion", "motion_stop"));
        _interactMotion = ToModel<Motion>(JsonReader.GetModel("motion", "motion_interact"));
        _inspectMotion = ToModel<Motion>(JsonReader.GetModel("motion", "motion_inspect"));
        _shootMotion = ToModel<Motion>(JsonReader.GetModel("motion", "motion_shoot"));
        _reportMotion = ToModel<Motion>(JsonReader.GetModel("motion", "motion_report"));

        _shotRule = ToModel<Shot>(JsonReader.GetModel("shot", "shot_rule"));

        _spyMinigame = ToModel<spyMinigame>(JsonReader.GetModel("spyMinigame", "minigame_spy"));
        _criminalMinigame = ToModel<criminalMinigame>(JsonReader.GetModel("criminalMinigame", "minigame_criminal"));
        _boomerMinigame = ToModel<boomerMinigame>(JsonReader.GetModel("boomerMinigame", "boomer_minigame"));
        _assassinMinigame = ToModel<assassinMinigame>(JsonReader.GetModel("assassinMinigame", "assassin_minigame"));
        _report = ToModel<report>(JsonReader.GetModel("report", "report_rule"));
    }

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    public static TValue ToModel<TValue>(object obj)
    {
        var json = JsonConvert.SerializeObject(obj);
        var model = JsonConvert.DeserializeObject<TValue>(json);
        return model;
    }
}