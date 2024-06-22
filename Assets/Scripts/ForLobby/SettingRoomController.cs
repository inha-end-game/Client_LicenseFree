using System;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using Newtonsoft.Json;
using TMPro;

public class SettingRoomController : MonoBehaviour
{
    [Header("NPC Info")]
    [SerializeField] private Int32 _npcCount = 100;

    public GameObject _roomSettingPopImage;
    public TMP_InputField _npcCountInput;
    public TextMeshProUGUI _npcCountText;
    private static bool _trySetting = false;
    private static bool _endSetting = false;
    private static bool _overCount = false;
    private bool setRoom = false;
    private static readonly String _responseName = responseType.SETTING_ROOM.ToString();

    void Start()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.startListenOnMessage(SettingRoomUpdated, _responseName);
    }

    void Update()
    {
        if (ModelManager._isRegister)
        {
            ModelManager._isRegister = false;
            Request();
        }
        if (_trySetting)
        {
            _roomSettingPopImage.gameObject.SetActive(true);
            _trySetting = false;
        }
        if (_endSetting)
        {
            _roomSettingPopImage.gameObject.SetActive(false);
            _endSetting = false;
        }
        if (_overCount)
        {
            _npcCountInput.text = "";
            _overCount = false;
        }
        
        _npcCountText.text = "NPC COUNT : " + ModelManager._npcCount;
    }

    void OnDestroy()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.stopListenOnMessage(SettingRoomUpdated, _responseName);
    }
    public void OnSettingButtonClicked()
    {
        _trySetting = true;
    }

    public void OnFinishButtonClicked()
    {
        int temp = _npcCount;
        string _npcCountString = _npcCountInput.text;
        _npcCount = int.Parse(_npcCountString);
        if (_npcCount < 1000 && 29 < _npcCount)
        {
            _endSetting = true;
            Request();
        }
        else
        {
            _npcCount = temp;
            _overCount = true;
        }
    }
    private void SettingRoomUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        SettingRoomResponse NPCInfo = SettingRoomResponse.CreateFromJSON(e.Data);
        _npcCount = NPCInfo.roomNpcs.Length;
        ModelManager._npcPosition.Clear();
        ModelManager._npcRotation.Clear();
        ModelManager._npcColor.Clear();
        ModelManager._users.Clear();
        for (int i = 0; i < NPCInfo.roomNpcs.Length; i++)
        {
            //Game 씬에게 초기위치 전달하기 위해 싱글턴변수에 저장
            ModelManager._npcPosition.Add(NPCInfo.roomNpcs[i].pos);
            ModelManager._npcRotation.Add(NPCInfo.roomNpcs[i].rot);
            ModelManager._npcColor.Add(NPCInfo.roomNpcs[i].color);
            ModelManager._users.Add(NPCInfo.roomNpcs[i].username, NPCInfo.roomNpcs[i]);
        }
        //Game 씬에게 초기위치 전달하기 위해 싱글턴변수에 저장
        ModelManager._npcCount = _npcCount;
    }
    public void Request()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        requestSettingRoom(networking);
    }
    private void requestSettingRoom(NetworkingController networking)
    {
        var request = new SettingRoomRequest(_npcCount);
        networking.sendRequest(request);
    }
    public class SettingRoomRequest : ClientRequest
    {
        public int roomId;
        public Int32 npcCount;
        public SettingRoomRequest(Int32 npcCount)
        {
            type = requestType.SETTING_ROOM.ToString();
            this.roomId = ModelManager._roomId;
            this.npcCount = npcCount;
        }
    }

    public class SettingRoomResponse
    {
        public RoomUser[] roomNpcs;
        public static SettingRoomResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<SettingRoomResponse>(jsonString);
        }
    }
}
