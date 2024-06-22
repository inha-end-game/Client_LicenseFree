using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.SocialPlatforms;

public class AddUserController : MonoBehaviour
{
	[Header("Player Info")]
	[Tooltip("Insert UserName")]
	[SerializeField] private string _username;
	[Tooltip("Insert NickName")]
	[SerializeField] public string _nickname;
	[Tooltip("Lobby Camera")]
	[SerializeField] private GameObject _roomJoinCamera;
	[SerializeField] private GameObject _cop;

	public int _roomId;
	public Dictionary<string, string> _userMap = new Dictionary<string, string>();

	public GameObject _lobbyBG;
	public GameObject _lobbyCanvas;
	public GameObject _chooseRoleUI;
	public GameObject _roomUI;
	public GameObject _userBlockBG;
	public GameObject _loginPopImage;
	public GameObject _startButton;
	public GameObject _roomSettingPopImage;
	public GameObject[] _criminalWantedPrefab = new GameObject[3];
	public Transform _criminalListContainer;
	public TextMeshPro _copNickname;
	public TextMeshProUGUI _roomIdText;
	public TMP_InputField _idInputField;
	public TMP_InputField _nicknameInputField;

	private CheckUserController _check;
	public AddUserResponse _cachedAddUserResponse;
	private bool _isRegister = false;
	private bool _wholeave = false;
	private bool _isNotCop;
	private int _userIndex;

	private static readonly string _responseName = responseType.ADD_USER.ToString();

	void Start()
	{
		NetworkingController networking = NetworkManager.getNetworkingController();
		networking.startListenOnMessage(AddUserUpdated, _responseName);
		_check = FindObjectOfType<CheckUserController>();
		_nicknameInputField.onEndEdit.AddListener(OnNicknameInputFieldEndEdit);

		foreach (Transform child in _criminalListContainer)
		{
			Destroy(child.gameObject);
		}
	}

	void Update()
	{
		if (_isRegister)
		{
			_roomJoinCamera.SetActive(true);
			_cop.GetComponent<Animator>().SetBool("PickPolice", true);
			_lobbyCanvas.SetActive(true);
			_lobbyBG.SetActive(false);
			_chooseRoleUI.SetActive(false);
			_roomUI.SetActive(false);
			_userBlockBG.SetActive(true);
			_roomSettingPopImage.SetActive(false);
			_loginPopImage.SetActive(false);
			_isRegister = false;
			_isNotCop = _check._isNotCop;
			if (_isNotCop)
			{
				_startButton.SetActive(false);
				_isNotCop = false;
			}
			else
			{
				ModelManager._isRegister = true;
				_startButton.SetActive(true);
			}
		}
		if (ModelManager._updateUserList)
		{
			//새로운 유저가 들어왔을때
			if (_cachedAddUserResponse != null)
			{
				UpdateRoomUser(_cachedAddUserResponse.roomUsers);
				_cachedAddUserResponse = null;
			}
			//재접속했을때
			if (ModelManager._roomUserList != null)
			{

				UpdateRoomUser(ModelManager._roomUserList);
				_userMap.Clear();
				for (int i = 0; i < ModelManager._roomUserList.Length; i++)
				{
					_userMap[ModelManager._roomUserList[i].username] = ModelManager._roomUserList[i].nickname;
				}
				ModelManager._roomUserList = null;
			}
			ModelManager._updateUserList = false;
		}
		if (_wholeave)
		{
			Destroy(_criminalListContainer.transform.GetChild(_userIndex).gameObject);
			_wholeave = false;
		}
		_roomIdText.text = "ROOM " + _roomId;
	}

	void OnDestroy()
	{
		NetworkingController networking = NetworkManager.getNetworkingController();
		networking.stopListenOnMessage(AddUserUpdated, _responseName);
	}

	private void UpdateRoomUser(RoomUser[] roomUsers)
	{
		foreach (Transform child in _criminalListContainer)
		{
			Destroy(child.gameObject);
		}

		for (int i = 0; i < roomUsers.Length; i++)
		{
			if (i == 0)
			{
				_copNickname.text = roomUsers[i].nickname;
			}
			else
			{
				GameObject criminalWanted = Instantiate(_criminalWantedPrefab[i - 1], _criminalListContainer);
				Debug.Log("방에서 재접속");
				TextMeshPro nicknameText = criminalWanted.transform.GetChild(0).GetComponent<TextMeshPro>();
				TextMeshPro reward = criminalWanted.transform.GetChild(2).GetComponent<TextMeshPro>();

				int rand = UnityEngine.Random.Range(1, 101);

				nicknameText.text = roomUsers[i].nickname;
				reward.text = "$" + $"{rand:D2},000,000";
			}
		}
	}
	private void OnNicknameInputFieldEndEdit(string input)
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			Request();
		}
	}


	private void AddUserUpdated(object sender, MessageEventArgs e)
	{
		_cachedAddUserResponse = AddUserResponse.CreateFromJSON(e.Data);
		Debug.Log(_cachedAddUserResponse.nickname + " has joined");

		_userMap.Clear();
		for (int i = 0; i < _cachedAddUserResponse.roomUsers.Length; i++)
		{
			string username = _cachedAddUserResponse.roomUsers[i].username;
			string nickname = _cachedAddUserResponse.roomUsers[i].nickname;
			_userMap[username] = nickname;
			Debug.Log(i + " : " + username + ", " + nickname);
		}

		_isRegister = true;
		ModelManager._updateUserList = true;
	}

	public void AddUserUpdated_Leave(int userIndex)
	{
		_userIndex = userIndex;
		_wholeave = true;
	}

	public void Request()
	{
		if (_nicknameInputField.text.Length > 7)
		{
			_check._existIdAndNickname = "닉네임은 7글자를 넘을 수 없습니다";
			_check._alreadyExistId = true;
		}
		else
		{
			NetworkingController networking = NetworkManager.getNetworkingController();
			RequestAddUser(networking);
		}
	}

	private void RequestAddUser(NetworkingController networking)
	{
		//Game 씬에 유저 정보전달
		_username = _idInputField.text;
		ModelManager._username = _username;
		_nickname = _nicknameInputField.text;
		ModelManager._nickname = _nickname;
		_roomId = _check._roomId;
		ModelManager._roomId = _roomId;
		var request = new AddUserRequest(_roomId, _username, _nickname);
		networking.sendRequest(request);
	}

	public class AddUserRequest : ClientRequest
	{
		public string username;
		public string nickname;
		public int roomId;
		public AddUserRequest(int roomid, string username, string nickname)
		{
			type = requestType.ADD_USER.ToString();
			this.username = username;
			this.nickname = nickname;
			this.roomId = roomid;
		}
	}

	public class AddUserResponse
	{
		public string nickname;
		public RoomUser[] roomUsers;
		public static AddUserResponse CreateFromJSON(string jsonString)
		{
			return JsonConvert.DeserializeObject<AddUserResponse>(jsonString);
		}
	}
}