using WebSocketSharp;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json;
using Unity.Mathematics;

public class StartController : MonoBehaviour
{
    public TextMeshProUGUI _text;
    public GameObject _lobbyCanvas;
    public GameObject _jobCanvas;
    public int _roomId;

    private string _roomState = roomState.NONE.ToString();
    private long _startTime;

    private static readonly string _responseName = responseType.START_ROOM.ToString();

    void Start()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.startListenOnMessage(StartRoomUpdated, _responseName);

        ModelManager._isStunned = false;
        ModelManager._whileStun = false;
        ModelManager._isDead = false;
        ModelManager._hasJustStunned = false;
        ModelManager._whileChat = false;
        ModelManager._missionPhase = 1;
        ModelManager._leftCriminal = 0;
        ModelManager._whileMission = false;
        ModelManager._roomId = 0;
    }

    void OnDestroy()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.stopListenOnMessage(StartRoomUpdated, _responseName);
    }

    void Update()
    {
        if (_roomState.Equals(roomState.READY.ToString()))
        {
            _jobCanvas.gameObject.SetActive(true);
            _lobbyCanvas.gameObject.SetActive(false);
            ChatController chat = GameObject.FindObjectOfType<ChatController>();
            chat._contentInputField.DeactivateInputField();
        }
        else if (_roomState.Equals(roomState.PLAY.ToString()))
        {
            SceneManager.LoadScene("Game");
            ModelManager._endOnce = true;
        }
    }

    private void StartRoomUpdated(object sender, MessageEventArgs e)
    {
        //Debug.Log(e.Data);
        StartRoomResponse startInfo = StartRoomResponse.CreateFromJSON(e.Data);
        Debug.Log(startInfo.roomState);
        _roomState = startInfo.roomState;
        _startTime = Convert.ToInt64(startInfo.nextStateAt);
        ModelManager._endTime = Convert.ToInt64(startInfo.nextStateAt);
    }

    public void Request()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        requestStartRoom(networking);
    }

    private void requestStartRoom(NetworkingController networking)
    {
        _roomId = ModelManager._roomId;
        var request = new StartRoomRequest(_roomId);
        networking.sendRequest(request);
    }

    public class StartRoomRequest : ClientRequest
    {
        public int roomId;
        public StartRoomRequest(int roomid)
        {
            type = requestType.START_ROOM.ToString();
            this.roomId = roomid;
        }
    }

    public class StartRoomResponse
    {
        public string roomState;
        public string nextStateAt;
        public static StartRoomResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<StartRoomResponse>(jsonString);
        }
    }
}