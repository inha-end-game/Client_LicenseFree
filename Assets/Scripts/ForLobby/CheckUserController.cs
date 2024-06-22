using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using WebSocketSharp;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheckUserController : MonoBehaviour
{
    [Header("Player Info")]
    [Tooltip("Insert Username")]
    public string _username;
    public int _roomId;

    public GameObject _ipCanvas;
    public GameObject _lobbyCanvas;
    public GameObject _loginPopImage;
    public TMP_InputField _idInputField;
    public TMP_InputField _nicknameInputField;
    public TextMeshProUGUI _warningText;
    public string _existIdAndNickname;
    public string _roomState;

    private bool _activeNicknameInputField = false;
    public bool _alreadyExistId = false;
    private bool _initialize = false;
    private bool _tryRejoining = false;
    public bool _isNotCop = false;

    private RoomListController _roomList;
    private static readonly string _responseName = responseType.CHECK_USER.ToString();


    void Start()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.startListenOnMessage(CheckUserUpdated, _responseName);
        _roomList = FindObjectOfType<RoomListController>();
        _idInputField.onEndEdit.AddListener(OnIdInputFieldEndEdit);
    }

    void Update()
    {
        if(_initialize)
        {
            _ipCanvas.gameObject.SetActive(false);
            _lobbyCanvas.gameObject.SetActive(true);
            _loginPopImage.gameObject.SetActive(true);
            _nicknameInputField.gameObject.SetActive(false);
            _warningText.gameObject.SetActive(false);
            _initialize = false;
        }
        if(_activeNicknameInputField)
        {
            _warningText.gameObject.SetActive(true);
            _warningText.text = "닉네임을 등록해주세요";
            _nicknameInputField.gameObject.SetActive(true);
            _nicknameInputField.Select();
            _activeNicknameInputField = false;
        }
        if(_alreadyExistId)
        {
            _warningText.gameObject.SetActive(true);
            _warningText.text = _existIdAndNickname;
            _alreadyExistId = false;
        }
        if(_tryRejoining)
        {
            _loginPopImage.gameObject.SetActive(false);
            _roomList.OnReloadButtonClicked();
            _tryRejoining = false;
        }
    }

    void OnDestroy()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.stopListenOnMessage(CheckUserUpdated, _responseName);
    }

    private void OnIdInputFieldEndEdit(string input)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Request();
        }
    }

    public void StartLogin(int roomId, string roomState, bool isNotCop)
    {
        _initialize = true;
        _roomId = roomId;
        _roomState = roomState;
        _isNotCop = isNotCop;
    }

    public void onXButtonClicked()
    {
        _tryRejoining = true;
    }

    private void CheckUserUpdated(object sender, MessageEventArgs e)
    {
        CheckUserResponse roomUser = CheckUserResponse.CreateFromJSON(e.Data);
        Debug.Log(roomUser.exist);
        if (!roomUser.exist)
        {
            if (_roomState != "PLAY")
            {
                _activeNicknameInputField = true;
            }
            else
            {
                _existIdAndNickname = _username + " 은 게임 중이 아닙니다.";
                _alreadyExistId = true;
            }
        }
        else
        {
            if (_roomState == "PLAY")
            {
                ModelManager._username = _username;
                ModelManager._roomId = _roomId;
                ModelManager._isDisconnectedCloseGame = true;
            }
            else
            {
                _existIdAndNickname = _username + " 은(는) 이미 방에 존재합니다. 새로운 ID를 입력하세요.";
                _alreadyExistId = true;
            }
        }
    }

    public void Request()
    {
        if (ContainsKoreanCharacters(_idInputField.text))
        {
            _existIdAndNickname = "ID는 영문자만 사용할 수 있습니다.";
            _alreadyExistId = true;
        }
        else
        {
            NetworkingController networking = NetworkManager.getNetworkingController();
            requestCheckUser(networking);
        }
    }

    private bool ContainsKoreanCharacters(string input)
    {
        foreach (char c in input)
        {
            if (char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter)
            {
                return true;
            }
        }
        return false;
    }

    private void requestCheckUser(NetworkingController networking)
    {
        _username = _idInputField.text;
        var request = new CheckUserRequest(_roomId, _username);
        networking.sendRequest(request);
    }

    public class CheckUserRequest : ClientRequest
    {
        public string username;
        public int roomId;
        public CheckUserRequest(int roomid, string username)
        {
            type = requestType.CHECK_USER.ToString();
            this.username = username;
            this.roomId = roomid;
        }
    }

    public class CheckUserResponse
    {
        public string nickname;
        public bool exist;
        public static CheckUserResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<CheckUserResponse>(jsonString);
        }
    }
}