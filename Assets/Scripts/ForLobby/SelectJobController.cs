using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using Newtonsoft.Json;
using TMPro;
using System.Linq;


public class SelectJobController : MonoBehaviour
{
    public GameObject _copInfo;
    public GameObject _spyInfo;
    public GameObject _assassinInfo;
    public GameObject _bomberInfo;

    private bool _selectedSpy = false;
    private bool _selectedBoomer = false;
    private bool _selectedAssassin = false;
    private bool _selectedCop = false;

    private static readonly string _responseName = responseType.SELECT_JOB.ToString();
    void Start()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.startListenOnMessage(SelectJobUpdated, _responseName);

        _copInfo.gameObject.SetActive(false);
        _spyInfo.gameObject.SetActive(false);
        _assassinInfo.gameObject.SetActive(false);
        _bomberInfo.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.stopListenOnMessage(SelectJobUpdated, _responseName);
    }

    void Update()
    {
        if (_selectedCop)
        {
            _copInfo.gameObject.SetActive(true);
            _selectedCop = false;
        }
        else if (_selectedSpy)
        {
            _spyInfo.gameObject.SetActive(true);
            _selectedSpy = false;
        }
        else if (_selectedBoomer)
        {
            _bomberInfo.gameObject.SetActive(true);
            _selectedBoomer = false;
        }
        else if (_selectedAssassin)
        {
            _assassinInfo.gameObject.SetActive(true);
            _selectedBoomer = false;
        }
    }

    private void SelectJobUpdated(object sender, MessageEventArgs e)
    {
        SelectJobResponse jobInfo = SelectJobResponse.CreateFromJSON(e.Data);
        Debug.Log(e.Data);
        ModelManager._userType = jobInfo.roomUserType;
        ModelManager._crimeType = jobInfo.crimeType;
        ModelManager._missionLocation = jobInfo.missionInfo;
        string job = jobInfo.roomUserType;
        RoomUser[] roomUsers = jobInfo.roomUsers;
        ModelManager._itemValid = ModelManager._userType == roomUserType.USER.ToString() ? 1 : 0;
        if (job == roomUserType.COP.ToString())
        {
            _selectedCop = true;
            //ModelManager._copPosition = jobInfo.pos;
        }
        else if (job == roomUserType.USER.ToString())
        {
            if (jobInfo.crimeType == crimeType.SPY.ToString())
            {
                _selectedSpy = true;
                //ModelManager._spyPosition = jobInfo.pos;
                job = crimeType.SPY.ToString();
            }
            else if (jobInfo.crimeType == crimeType.BOOMER.ToString())
            {
                _selectedBoomer = true;
                //ModelManager._boomerPosition = jobInfo.pos;
                job = crimeType.BOOMER.ToString();
            }
            else if (jobInfo.crimeType == crimeType.ASSASSIN.ToString())
            {
                ModelManager._targetID = jobInfo.targetInfo.ToList();
                _selectedAssassin = true;
                job = crimeType.ASSASSIN.ToString();
            }
        }
        //초기위치 지정
        for (int i = 0; i < roomUsers.Length; i++)
        {
            if (roomUsers[i].username == ModelManager._username)
            {
                ModelManager._userColor = roomUsers[i].color;
            }
            ModelManager._users.Add(roomUsers[i].username, roomUsers[i]);
            if (roomUsers[i].roomUserType == roomUserType.COP.ToString())
            {
                ModelManager._copPosition = roomUsers[i].pos;
                ModelManager._copRotation = roomUsers[i].rot;
                ModelManager._copNickname = roomUsers[i].nickname;
            }
            else if (roomUsers[i].roomUserType == roomUserType.USER.ToString())
            {
                ModelManager._leftCriminal++;
                if (roomUsers[i].crimeType == crimeType.SPY.ToString())
                {
                    ModelManager._spyPosition = roomUsers[i].pos;
                    ModelManager._spyRotation = roomUsers[i].rot;
                    ModelManager._spyColor = roomUsers[i].color;
                    ModelManager._spyNickname = roomUsers[i].nickname;
                }
                else if (roomUsers[i].crimeType == crimeType.BOOMER.ToString())
                {
                    ModelManager._boomerPosition = roomUsers[i].pos;
                    ModelManager._boomerRotation = roomUsers[i].rot;
                    ModelManager._boomerColor = roomUsers[i].color;
                    ModelManager._boomerNickname = roomUsers[i].nickname;
                }
                else if (roomUsers[i].crimeType == crimeType.ASSASSIN.ToString())
                {
                    ModelManager._assassinPosition = roomUsers[i].pos;
                    ModelManager._assassinRotation = roomUsers[i].rot;
                    ModelManager._assassinColor = roomUsers[i].color;
                    ModelManager._assasssinNickname = roomUsers[i].nickname;
                }
            }
        }
    }
    public class SelectJobResponse
    {
        public string roomUserType;
        public rVector3 pos;
        public string crimeType;
        public RoomUser[] roomUsers;
        public Dictionary<int, rVector3> missionInfo;
        public string[] targetInfo;
        public static SelectJobResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<SelectJobResponse>(jsonString);
        }
    }
}