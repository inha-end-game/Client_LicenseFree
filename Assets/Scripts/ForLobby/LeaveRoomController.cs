using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.SocialPlatforms;

public class LeaveRoomController : MonoBehaviour
{
    [SerializeField] private GameObject _roomJoinCamera;
    private string _username;
    private int _roomId;
    private AddUserController _addUser;
    private LeaveRoomResponse _leaveRoom;
    private static bool _ifLeave = false;
    private static readonly string _responseName = responseType.LEAVE_ROOM.ToString();

    void Start()
	{
		NetworkingController networking = NetworkManager.getNetworkingController();
		networking.startListenOnMessage(LeaveRoomUpdated, _responseName);
		_addUser = FindObjectOfType<AddUserController>();
	}

    void Update()
    {
        if(_ifLeave)
        {
            if (!_leaveRoom.checkLeave)
            {
                foreach (var leaveUsername in _leaveRoom.leaveUsername)
                {
                    if (_addUser._userMap.ContainsKey(leaveUsername))
                    {
                        for (int i = 0; i < _addUser._criminalListContainer.transform.childCount; i++)
                        {
                            GameObject criminalWanted = _addUser._criminalListContainer.transform.GetChild(i).gameObject;
                            if (criminalWanted.transform.GetChild(0).GetComponent<TextMeshPro>().text == _addUser._userMap[leaveUsername])
                            {
                                _addUser.AddUserUpdated_Leave(i);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                EntryController._chooseRole = true;
                _addUser._copNickname.text = "";
                ModelManager._roomId = 0;
                foreach (Transform child in _addUser._criminalListContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            _ifLeave = false;
        }
    }

    void OnDestroy()
	{
		NetworkingController networking = NetworkManager.getNetworkingController();
		networking.stopListenOnMessage(LeaveRoomUpdated, _responseName);
	}

	public void Request()
	{
		NetworkingController networking = NetworkManager.getNetworkingController();
		RequestLeaveRoom(networking);
	}

    public void RequestLeaveRoom(NetworkingController networking)
    {
        _username = ModelManager._username;
        _roomId = ModelManager._roomId;
        var request = new LeaveRoomRequest(_username, _roomId);
        networking.sendRequest(request);
    }

    public void LeaveRoomUpdated(object sender, MessageEventArgs e)
    {
        _leaveRoom = LeaveRoomResponse.CreateFromJSON(e.Data);
        _ifLeave = true;
    }

    public class LeaveRoomRequest : ClientRequest
    {
        public string leaveUsername;
        public int roomId;
        public LeaveRoomRequest(string leaveUsername, int roomId)
        {
            type = requestType.LEAVE_ROOM.ToString();
            this.leaveUsername = leaveUsername;
            this.roomId = roomId;
        }
    }

    public class LeaveRoomResponse
    {
        public string[] leaveUsername;
        public bool checkLeave;
        public static LeaveRoomResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<LeaveRoomResponse>(jsonString);
        }
    }
}