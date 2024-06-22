using Newtonsoft.Json;
using UnityEngine;
using TMPro;
using System;
using WebSocketSharp;
using System.Collections;
using System.Collections.Generic;

public class EventController : MonoBehaviour
{
    [Header("Event Screen")]
    [SerializeField] private List<TextMeshPro> _screen;
    [Header("Event UI for Cop")]
    [SerializeField] private TextMeshProUGUI _eventMsg;
    [SerializeField] private GameObject _eventCanvas;
    [SerializeField] private GameObject _preview;
    private int _animNum;
    private long _nextAnimAt;
    private static readonly string _responseName = responseType.EVENT_INFO.ToString();
    private bool _startCount = false;
    void Start()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.startListenOnMessage(EventUpdated, _responseName);
        CanvasInit();
        ScreenInit();
    }

    void OnDestroy()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.stopListenOnMessage(EventUpdated, _responseName);
    }
    void Update()
    {
        if (_startCount)
        {
            StartCoroutine("CanvasEvent");
            StartCoroutine("ScreenEvent");
            _startCount = false;
        }
    }
    //Cop UI 코루틴
    IEnumerator CanvasEvent()
    {
        Debug.Log("start coroutine");
        //이벤트 발생시 cop UI활성화
        _eventCanvas.SetActive(true);
        float leftTime = (_nextAnimAt - PingManager.GameTime) / 1000.0f;
        while (leftTime >= -5)
        {
            leftTime -= Time.deltaTime;
            //Cop UI update
            _preview.GetComponent<Animator>().SetInteger("motion", _animNum);
            if (leftTime > 0)
                _eventMsg.text = "NEXT EVENT \nin " + Mathf.Ceil(leftTime).ToString() + "sec";
            else
                _eventMsg.text = "Event \nin Progress...";
            yield return new WaitForFixedUpdate();
        }
        CanvasInit();
    }
    private void CanvasInit()
    {
        _preview.GetComponent<Animator>().SetInteger("motion", 0);
        _eventCanvas.SetActive(false);
    }
    //스크린용 코루틴
    IEnumerator ScreenEvent()
    {
        Debug.Log("start coroutine");

        float leftTime = (_nextAnimAt - PingManager.GameTime) / 1000.0f;
        while (leftTime >= 0)
        {
            leftTime -= Time.deltaTime;
            //Screen update
            foreach (TextMeshPro screen in _screen)
            {
                screen.text =  Mathf.Ceil(leftTime).ToString() + " 초 후,\n춤 \"" + (_animNum-4) + "\" 번을 추십시오";
            }
            yield return new WaitForFixedUpdate();
        }
        ScreenInit();
    }
    private void ScreenInit()
    {
        foreach (TextMeshPro screen in _screen)
        {
            screen.text = "범죄자의 체포를 위해\n시민분들의 협조 바랍니다.";
        }
    }
    public void EventUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log("Server says: " + e.Data);

        EventInfoResponse eventInfo = EventInfoResponse.CreateFromJSON(e.Data);
        _animNum = eventInfo.animNum;
        _nextAnimAt = Convert.ToInt64(eventInfo.nextAnimAt);
        _startCount = true;
    }
    public class EventInfoResponse
    {
        public Int32 animNum;
        public string nextAnimAt;
        public static EventInfoResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<EventInfoResponse>(jsonString);
        }
    }
}
