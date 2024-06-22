using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Drawing;
using UnityEngine.UIElements;

public class SpySpecialMinigame : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _leftTime;
    [SerializeField] private TextMeshProUGUI _hacking;
    [SerializeField] private TextMeshProUGUI _enterCode;
    [SerializeField] private List<GameObject> _arrows;
    [SerializeField] private Sprite _rightArrow;
    [SerializeField] private Sprite _leftArrow;
    [SerializeField] private Sprite _upArrow;
    [SerializeField] private Sprite _downArrow;
    private List<KeyCode> _codes = new List<KeyCode>();
    private int _missionSuccess = 0;
    private int _count = 0;
    private int _codeCount = 0;
    private bool _inputCode = false;
    private int _missionTimeLimit = ModelManager._spyMinigame.pressTimeLimit - 1;
    private GameObject InputSpyCode;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        for (int i = 0; i < _arrows.Count; i++)
        {
            _arrows[i].SetActive(false);
        }
        StartCoroutine("ResetCode");
        InputSpyCode = gameObject.transform.Find("InputSpyCode").gameObject;
        InputSpyCode.SetActive(false);
        InputSpyCode.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text
            = ModelManager._spyCode[ModelManager._roomId % 3][0].ToString();
        InputSpyCode.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text
            = ModelManager._spyCode[ModelManager._roomId % 3][1].ToString();
        InputSpyCode.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text
            = ModelManager._spyCode[ModelManager._roomId % 3][2].ToString();
    }
    // Update is called once per frame
    void Update()
    {
        ReplaceMission();
        if (!ModelManager._isDisconnected)
        {
            SpecialMission();
        }
    }
    private void SpecialMission()
    {
        if (_missionSuccess == 3)
        {
            _inputCode = true;
            StopCoroutine("MissionCountdown");
            InputSpyCode.SetActive(true);
            InputCode(ModelManager._roomId % 3);
        }
        //키 입력 게임
        if (_count < 10 && !_inputCode)
        {
            KeyCode inputKey = GetInputKey();
            //키 입력이 들어왔을때
            if (inputKey != KeyCode.None)
            {
                //첫 키코드와 같다면 해당 키코드 제거
                if (_codes[_count] == inputKey)
                {
                    _arrows[_count].SetActive(false);
                    _count++;
                }
                // 틀리면 미니게임 실패
                else
                {
                    FinishMission(missionState.FAIL.ToString());
                }
            }
        }
        //모두 맞출경우 코드 리셋 후 성공 카운트
        else if (_count == 10)
        {
            _count = 0;
            _missionSuccess++;
            StopCoroutine("MissionCountdown");
            StartCoroutine("ResetCode");
        }
    }
    private void InputCode(int code)
    {
        switch (code)
        {
            case 0:
                if (_codeCount == 0)
                {
                    if (Input.GetKeyDown(KeyCode.N))
                    {
                        InputSpyCode.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                        _codeCount++;
                    }
                }
                if (_codeCount == 1)
                {
                    if (Input.GetKeyDown(KeyCode.P))
                    {
                        InputSpyCode.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
                        _codeCount++;
                    }
                }
                if (_codeCount == 2)
                {
                    if (Input.GetKeyDown(KeyCode.C))
                    {
                        InputSpyCode.transform.GetChild(2).GetChild(0).gameObject.SetActive(true);
                        _codeCount++;
                        _enterCode.text = "MATCHING SUCCESS";
                        Invoke("FinishMissionSuccess", 2);
                    }
                }
                break;
            case 1:
                if (_codeCount == 0)
                {
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        InputSpyCode.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                        _codeCount++;
                    }
                }
                if (_codeCount == 1)
                {
                    if (Input.GetKeyDown(KeyCode.P))
                    {
                        InputSpyCode.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
                        _codeCount++;
                    }
                }
                if (_codeCount == 2)
                {
                    if (Input.GetKeyDown(KeyCode.Y))
                    {
                        InputSpyCode.transform.GetChild(2).GetChild(0).gameObject.SetActive(true);
                        _codeCount++;
                        _enterCode.text = "MATCHING SUCCESS";
                        Invoke("FinishMissionSuccess", 2);
                    }
                }
                break;
            case 2:
                if (_codeCount == 0)
                {
                    if (Input.GetKeyDown(KeyCode.C))
                    {
                        InputSpyCode.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                        _codeCount++;
                    }
                }
                if (_codeCount == 1)
                {
                    if (Input.GetKeyDown(KeyCode.O))
                    {
                        InputSpyCode.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
                        _codeCount++;
                    }
                }
                if (_codeCount == 2)
                {
                    if (Input.GetKeyDown(KeyCode.P))
                    {
                        InputSpyCode.transform.GetChild(2).GetChild(0).gameObject.SetActive(true);
                        _codeCount++;
                        _enterCode.text = "MATCHING SUCCESS";
                        Invoke("FinishMissionSuccess", 2);
                    }
                }
                break;
            default:
                break;
        }
    }
    IEnumerator ResetCode()
    {
        float time = 0;
        _leftTime.text = "";
        _hacking.text = "[ HACKING. ]";
        time = 0;
        while (time < 1)
        {
            time += Time.deltaTime;
            RandomPrint();
            yield return new WaitForFixedUpdate();
        }
        _hacking.text = "[ HACKING.. ]";
        time = 0;
        while (time < 1)
        {
            time += Time.deltaTime;
            RandomPrint();
            yield return new WaitForFixedUpdate();
        }
        _hacking.text = "[ HACKING... ]";
        time = 0;
        while (time < 1)
        {
            time += Time.deltaTime;
            RandomPrint();
            yield return new WaitForFixedUpdate();
        }
        _hacking.text = "[ HACKING.... ]";
        time = 0;
        while (time < 1)
        {
            time += Time.deltaTime;
            RandomPrint();
            yield return new WaitForFixedUpdate();
        }
        _hacking.text = "[ PRESS ARROWS TO HACK ]";
        _codes.Clear();
        for (int i = 0; i < 10; i++)
        {
            _arrows[i].SetActive(true);
            switch (Random.Range(0, 4))
            {
                case 0:
                    _codes.Add(KeyCode.LeftArrow);
                    _arrows[i].GetComponent<UnityEngine.UI.Image>().sprite = _leftArrow;
                    break;
                case 1:
                    _codes.Add(KeyCode.UpArrow);
                    _arrows[i].GetComponent<UnityEngine.UI.Image>().sprite = _upArrow;
                    break;
                case 2:
                    _codes.Add(KeyCode.DownArrow);
                    _arrows[i].GetComponent<UnityEngine.UI.Image>().sprite = _downArrow;
                    break;
                case 3:
                    _codes.Add(KeyCode.RightArrow);
                    _arrows[i].GetComponent<UnityEngine.UI.Image>().sprite = _rightArrow;
                    break;
                default:
                    break;
            }
        }
        StartCoroutine("MissionCountdown");
        yield return null;
    }
    private void RandomPrint()
    {
        for (int i = 0; i < 10; i++)
        {
            _arrows[i].SetActive(true);
            switch (Random.Range(0, 4))
            {
                case 0:
                    _arrows[i].GetComponent<UnityEngine.UI.Image>().sprite = _leftArrow;
                    break;
                case 1:
                    _arrows[i].GetComponent<UnityEngine.UI.Image>().sprite = _upArrow;
                    break;
                case 2:
                    _arrows[i].GetComponent<UnityEngine.UI.Image>().sprite = _downArrow;
                    break;
                case 3:
                    _arrows[i].GetComponent<UnityEngine.UI.Image>().sprite = _rightArrow;
                    break;
                case 4:
                    _arrows[i].GetComponent<UnityEngine.UI.Image>().sprite = null;
                    break;
                default:
                    break;
            }
        }
    }
    private KeyCode GetInputKey()
    {
        if (!ModelManager._whileChat)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                return KeyCode.LeftArrow;
            if (Input.GetKeyDown(KeyCode.RightArrow))
                return KeyCode.RightArrow;
            if (Input.GetKeyDown(KeyCode.UpArrow))
                return KeyCode.UpArrow;
            if (Input.GetKeyDown(KeyCode.DownArrow))
                return KeyCode.DownArrow;
            return KeyCode.None;
        }
        return KeyCode.None;
    }
    private void FinishMission(string missionState)
    {
        // 실패시 종료
        sendRequestMission(ModelManager._missionLocation[ModelManager._missionPhase], missionState);
        // 스택 초기화하고 UI비활성화
        _missionSuccess = 0;
        _count = 0;
        _codeCount = 0;
        Destroy(gameObject);
        StopCoroutine("MissionCountdown");
    }
    private void FinishMissionSuccess()
    {
        // 실패시 종료
        sendRequestMission(ModelManager._missionLocation[ModelManager._missionPhase], missionState.CLEAR.ToString());
        // 스택 초기화하고 UI비활성화
        _missionSuccess = 0;
        _codeCount = 0;
        _count = 0;
        Destroy(gameObject);
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
    //미션 남은시간 카운트 코루틴
    IEnumerator MissionCountdown()
    {
        float missionTime = _missionTimeLimit;
        while (missionTime > 0)
        {
            missionTime -= Time.deltaTime;
            _leftTime.text = Mathf.Ceil(missionTime).ToString();
            yield return new WaitForFixedUpdate();
        }
        //시간 소진시 실패
        FinishMission(missionState.FAIL.ToString());
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
