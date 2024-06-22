using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.UI;

public class BoomerSpecialMinigame : MonoBehaviour
{
    [SerializeField] private GameObject _bar;
    private float _gage = 0;
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        BoomerMission();
    }
    private void BoomerMission()
    {
        if (Input.GetKey(KeyCode.E))
        {
            _gage += Time.deltaTime;
            _bar.GetComponent<Image>().fillAmount = _gage / 10;
            if (_gage >= 10.0f)
            {
                FinishMission(missionState.CLEAR.ToString());
            }
        }
        else
        {
            FinishMission(missionState.FAIL.ToString());
        }
    }
    private void FinishMission(string missionState)
    {
        // 실패시 종료
        sendRequestMission(ModelManager._missionLocation[ModelManager._missionPhase], missionState);
        // 스택 초기화하고 UI비활성화
        Destroy(gameObject);
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
}
