using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using TMPro;

public class CommonMinigame : MonoBehaviour
{
    [SerializeField] private GameObject _bar;
    [SerializeField] private GameObject _spyCode;
    [SerializeField] private GameObject _target;
    [SerializeField] private List<GameObject> _successMission;
    private int _barSpeed = 1;
    private int _missionSuccess = 0;
    private bool _justCleared = true;
    private float x = 0;
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        ReplaceMission();
        if (!ModelManager._isDisconnected)
        {
            CommonMission();
        }
    }
    private void CommonMission()
    {
        if (_justCleared)
        {
            if (_missionSuccess == 5)
            {
                //스파이일 경우 코드 출력
                if (ModelManager._crimeType == crimeType.SPY.ToString())
                {
                    GameObject code = Instantiate(_spyCode, gameObject.transform);
                    code.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                        ModelManager._spyCode[ModelManager._roomId % 3][ModelManager._missionPhase - 1].ToString();
                    Invoke("FinishMissionSuccess", 3);
                }
                else
                {
                    FinishMission(missionState.CLEAR.ToString());
                }
                _justCleared = false;
            }
            ResetSpeed();
            MoveBar();
            if (Input.GetKeyDown(KeyCode.E) && !ModelManager._whileChat)
            {
                if (70 >= Math.Abs(_bar.gameObject.GetComponent<RectTransform>().anchoredPosition.x
                    - _target.gameObject.GetComponent<RectTransform>().anchoredPosition.x))
                {
                    _successMission[_missionSuccess].SetActive(true);
                    _missionSuccess++;
                    StartCoroutine("ReplaceBar");
                }
                else
                {
                    FinishMission(missionState.FAIL.ToString());
                }
            }

        }
        if (Input.GetKeyDown(KeyCode.Escape) && !ModelManager._whileChat)
        {
            FinishMission(missionState.FAIL.ToString());
        }
    }
    private void ReplaceMission()
    {
        if (ModelManager._isReconnected)
        {
            if (ModelManager._whileMission)
            {
                FinishMission(missionState.FAIL.ToString());
            }
        }
    }
    IEnumerator ReplaceBar()
    {
        _target.SetActive(false);
        _target.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(UnityEngine.Random.Range(-360f, 360f), 0);
        yield return new WaitForSeconds(2);
        _target.SetActive(true);
    }
    private void FinishMission(string missionState)
    {
        // 실패시 종료
        sendRequestMission(ModelManager._missionLocation[ModelManager._missionPhase], missionState);
        // 스택 초기화하고 UI비활성화
        _missionSuccess = 0;
        Destroy(gameObject);
    }
    private void FinishMissionSuccess()
    {
        // 실패시 종료
        sendRequestMission(ModelManager._missionLocation[ModelManager._missionPhase], missionState.CLEAR.ToString());
        // 스택 초기화하고 UI비활성화
        _missionSuccess = 0;
        Destroy(gameObject);
    }
    //성공할때마다 바 속도 증가
    private void ResetSpeed()
    {
        switch (_missionSuccess)
        {
            case 0:
                _barSpeed = ModelManager._criminalMinigame.firstMovingBarSpeed / 50;
                break;
            case 1:
                _barSpeed = (int)Mathf.Lerp(_barSpeed, ModelManager._criminalMinigame.secondMovingBarSpeed / 50, Time.deltaTime);
                break;
            case 2:
                _barSpeed = ModelManager._criminalMinigame.secondMovingBarSpeed / 50;
                break;
            case 3:
                _barSpeed = (int)Mathf.Lerp(_barSpeed, ModelManager._criminalMinigame.thirdMovingBarSpeed / 50, Time.deltaTime);
                break;
            case 4:
                _barSpeed = ModelManager._criminalMinigame.thirdMovingBarSpeed / 50;
                break;
            default:
                break;
        }
    }
    private void MoveBar()
    {
        x = 0;
        x += 400.0f * Mathf.Sin(Time.time * _barSpeed);
        _bar.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, 0);
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
