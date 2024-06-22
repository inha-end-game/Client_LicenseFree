using Newtonsoft.Json;
using UnityEngine;
using TMPro;
using System;
using WebSocketSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem.Controls;
public class LogController : MonoBehaviour
{
    [SerializeField] private GameObject _killLog;
    [SerializeField] private GameObject _arrestLog;
    [SerializeField] private GameObject _reportLog;
    [SerializeField] private Canvas _canvas;
    private string _shotTarget;
    private string _killedTarget;
    private bool _hasShot = false;
    private bool _hasKilled = false;
    private bool _haskilledTarget = false;
    private long _highlightStartAt;
    private bool _justReport = false;
    private List<string> _reportUsername = new List<string>();
    private List<string> _reportTargetUsername = new List<string>();
    private List<GameObject> _logs = new List<GameObject>();
    private static readonly string _shotResponseName = responseType.SHOT.ToString();
    private static readonly string _KillResponseName = responseType.ASSASSIN_KILL.ToString();
    private static readonly string _ReportResponseName = responseType.REPORT_USER.ToString();
    void Start()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.startListenOnMessage(ShotUpdated, _shotResponseName);
        networking.startListenOnMessage(KilledUpdated, _KillResponseName);
        networking.startListenOnMessage(ReportUpdated, _ReportResponseName);
    }
    void OnDestroy()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.stopListenOnMessage(ShotUpdated, _shotResponseName);
        networking.stopListenOnMessage(KilledUpdated, _KillResponseName);
        networking.stopListenOnMessage(ReportUpdated, _ReportResponseName);
    }
    void Update()
    {
        if (_hasShot)
        {
            _hasShot = false;
            StartCoroutine("UpdateLog", 0);
        }
        if (_hasKilled)
        {
            _hasKilled = false;
            StartCoroutine("UpdateLog", 1);
        }
        if (_justReport)
        {
            _justReport = false;
            StartCoroutine("ReportResult");
        }
        PrintLog();
    }
    void PrintLog()
    {
        int offset = 0;
        foreach (GameObject log in _logs)
        {
            log.transform.SetParent(_canvas.transform);
            log.GetComponent<RectTransform>().anchoredPosition
                = new Vector2(275, 430 - offset);
            offset += 50;
        }
    }
    IEnumerator UpdateLog(int i)
    {
        GameObject log = new GameObject();
        //0 : 사격(경찰) 1 : 암살(어쌔신) 2 : 신고
        switch (i)
        {
            case 0:
                log = Instantiate(_arrestLog);
                log.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = ModelManager._copNickname;
                log.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text
                    = _haskilledTarget ? "암살 타겟" : ModelManager._users[_shotTarget].nickname;
                _logs.Add(log);
                _haskilledTarget = false;
                break;
            case 1:
                log = Instantiate(_killLog);
                log.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = ModelManager._assasssinNickname;
                log.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "암살 타겟";
                _logs.Add(log);
                break;
            case 2:
                log = Instantiate(_reportLog);
                log.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = ModelManager._users[_reportUsername[0]].nickname;
                log.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = ModelManager._users[_reportTargetUsername[0]].nickname;
                _reportUsername.RemoveAt(0);
                _reportTargetUsername.RemoveAt(0);
                _logs.Add(log);
                break;
            default:
                break;
        }
        yield return new WaitForSeconds(10);
        Destroy(log);
        _logs.RemoveAt(0);
    }
    IEnumerator ReportResult()
    {
        float reportTime = (_highlightStartAt - PingManager.GameTime) / 1000.0f;
        while (reportTime >= 0)
        {
            reportTime -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        StartCoroutine("UpdateLog", 2);
    }
    private void ShotUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        ShotResponse ShotInfo = ShotResponse.CreateFromJSON(e.Data);
        _shotTarget = ShotInfo.targetUsername;
        _hasShot = true;
        _haskilledTarget = ShotInfo.checkAssassinTarget;
    }
    private void KilledUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        AssassinKillResponse KilledInfo = AssassinKillResponse.CreateFromJSON(e.Data);
        _killedTarget = KilledInfo.targetUsername;
        _hasKilled = true;
    }
    private void ReportUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        ReportUserResponse reportInfo = ReportUserResponse.CreateFromJSON(e.Data);
        _justReport = true;
        _reportUsername.Add(reportInfo.reportUsername);
        _reportTargetUsername.Add(reportInfo.targetUsername);
        _highlightStartAt = Convert.ToInt64(reportInfo.highlightStartAt);
    }
    public class ShotResponse
    {
        public string targetUsername;
        public string targetUserType;
        public int aliveUserCount;
        public bool checkAssassinTarget;
        public static ShotResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<ShotResponse>(jsonString);
        }
    }
    public class AssassinKillResponse
    {
        public string targetUsername;
        public static AssassinKillResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<AssassinKillResponse>(jsonString);
        }
    }
    public class ReportUserResponse
    {
        public string reportUsername;
        public string nextReportAvailAt;
        public string targetUsername;
        public string highlightStartAt;
        public string highlightEndAt;
        public string reportMessage;
        public static ReportUserResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<ReportUserResponse>(jsonString);
        }
    }
}
