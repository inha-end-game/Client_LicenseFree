using System;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using Unity.Mathematics;

public class PingManager : MonoBehaviour
{
    //유저 인풋 딜레이용 RTT값
    private static long _avgRtt;
    public static long AvgRtt { get { return _avgRtt; } }
    //게임시간
    private static long _gameTime;
    public static long GameTime { get { return _gameTime; } }
    private long _sentTime;
    private long _currentTime;
    private long _clientTime;
    private long _serverTime;
    private long _avgDelay;
    private float _reqSentPeriod = 0.0f;
    //데이터 수
    private long _rttCount = 0;
    private long _delayCount = 0;
    //편차제곱 합
    //private long _rttSum = 0;
    //private long _delaySum = 0;
    //표준편차
    //private double _rttDeviation = 0;
    //private double _delayCountDeviation = 0;
    public static bool _pingUpdated = false;
    private static readonly String _responseName = responseType.PING.ToString();
    private static PingManager _instance;
    // Start is called before the first frame update
    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    void Start()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.startListenOnMessage(PingUpdated, _responseName);

        _avgRtt = 0;
        _avgDelay = 0;
    }
    void OnDestroy()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.stopListenOnMessage(PingUpdated, _responseName);
    }
    // Update is called once per frame
    void Update()
    {
        if (ModelManager._whileConnected)
        {
            //시간 동기화
            DateTimeOffset now = DateTimeOffset.UtcNow;
            _currentTime = now.ToUnixTimeMilliseconds();
            _gameTime = _currentTime + _avgDelay;

            //1초마다 pingReq보냄
            _reqSentPeriod += Time.deltaTime;
            if (_reqSentPeriod >= 1.0f)
            {
                _reqSentPeriod = 0.0f;
                Request();
            }
            if (_pingUpdated)
            {
                CheckConnect();
            }
            //Debug.Log("시간차이" + (_currentTime - _serverTime).ToString());
        }
    }
    private void CheckConnect()
    {
        //서버 시간과 현재 시간이 5초 이상 차이날 경우(서버 시간 갱신이 3초이상 안되는경우)
        if ((_currentTime - _serverTime) > 6000 && ModelManager._checkOnce)
        {
            ModelManager._whileConnected = false;
            Debug.Log("연결끊김");
            _pingUpdated = false;
            if (ModelManager._whileGame)
            {
                //Debug.Log("게임중 연결끊김");
                //sendDisconnectRequest();
                ModelManager._isDisconnectedInGame = true;
                ModelManager._checkOnce = false;
            }
            else
            {
                Debug.Log("로비에서 연결끊김");
                //sendDisconnectRequest();
                ModelManager._isDisconnectedInLobby = true;
                ModelManager._checkOnce = false;
            }
        }
    }
    private void PingUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data + " / 현재시간 " + _gameTime);
        //받은시간에서 빼서 rtt계산
        _currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long rtt = _currentTime - _sentTime;
        //이상치(+-1초)가 아닐경우 rtt평균합산
        if (math.abs(_avgRtt - rtt) < 100)
        {
            _avgRtt = ((_avgRtt * _rttCount) + rtt) / ++_rttCount;
            /*표준편차 계산
            _rttSum += (_avgRtt - rtt) * (_avgRtt - rtt);
            _rttDeviation = Math.Sqrt(_rttSum/_rttCount);
            */
        }
        PingResponse pingInfo = PingResponse.CreateFromJSON(e.Data);
        _serverTime = Convert.ToInt64(pingInfo.serverTime);
        _clientTime = _sentTime + (rtt / 2);
        long delay = _serverTime - _clientTime;
        //이상치(+-1초)가 아닐경우 delay평균합산
        _avgDelay = (_avgDelay * _delayCount + delay) / ++_delayCount;
        if (math.abs(_avgDelay - delay) < 100)
        {

        }
        //Debug.Log("평균 rtt = " + _avgRtt + "평균 delay = " + _avgDelay);
        _pingUpdated = true;
    }
    public void Request()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        requestPing(networking);
        //보낸시간 저장
        _sentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //Debug.Log("보낸시간 : " + _sentTime);
    }
    private void requestPing(NetworkingController networking)
    {

        var request = new PingRequest(0);
        networking.sendRequest(request);
    }
    public class PingRequest : ClientRequest
    {
        public PingRequest(int sentTime)
        {
            type = requestType.PING.ToString();
        }
    }
    public void sendDisconnectRequest()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        requestTest(networking);
    }
    private void requestTest(NetworkingController networking)
    {
        var request = new TestRequest();
        networking.sendRequest(request);
    }
    public class TestRequest : ClientRequest
    {
        public TestRequest()
        {
            type = requestType.TEST.ToString();
        }
    }
    public class PingResponse
    {
        public string serverTime;
        public static PingResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<PingResponse>(jsonString);
        }
    }
}
