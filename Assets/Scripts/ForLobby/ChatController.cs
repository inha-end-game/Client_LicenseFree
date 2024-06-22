using System;
using System.Collections;
using System.Collections.Generic;
using WebSocketSharp;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ChatController : MonoBehaviour
{
    public TMP_InputField _contentInputField;
    public GameObject _chatPrefab;
    public Transform _chatContentTransform;

    private string _nickname;
    private string _content;
    private int _roomId;
    private string _chatAt;
    private bool _sendChat = false;

    private ChatResponse _cachedChatResponse;
    private static readonly String _responseName = responseType.CHAT.ToString();

    void Start()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.startListenOnMessage(ChatUpdated, _responseName);

        _contentInputField.onSelect.AddListener(delegate { ActivateChatMode(true); });
        _contentInputField.onDeselect.AddListener(delegate { ActivateChatMode(false); });
        _contentInputField.onEndEdit.AddListener(delegate (string text) { if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) { Request(); ActivateChatMode(false); }});
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!EventSystem.current.currentSelectedGameObject || EventSystem.current.currentSelectedGameObject != _contentInputField.gameObject)
            {
                _contentInputField.Select();
            }
        }
        if (_sendChat)
        {
            if (_cachedChatResponse != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_chatContentTransform);
                GameObject newChat = Instantiate(_chatPrefab, _chatContentTransform);

                TextMeshProUGUI[] texts = newChat.GetComponentsInChildren<TextMeshProUGUI>();
                long a = Convert.ToInt64(_cachedChatResponse.chatAt);
                DateTime chatAt = DateTimeOffset.FromUnixTimeSeconds(a / 1000).UtcDateTime;
                texts[0].text = "[" + chatAt.ToString("hh:mm:ss") + "]";
                texts[1].text = _cachedChatResponse.nickname;
                texts[2].text = _cachedChatResponse.content;

                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_chatContentTransform);

                ScrollRect scrollRect = _chatContentTransform.GetComponentInParent<ScrollRect>();
                if (scrollRect != null)
                {
                    scrollRect.normalizedPosition = new Vector2(0, 0);
                }

                _cachedChatResponse = null;
            }
            _sendChat = false;
        }
    }

    void OnDestroy()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.stopListenOnMessage(ChatUpdated, _responseName);
    }

    public void ActivateChatMode(bool isActive)
    {
        ModelManager._whileChat = isActive;
    }

    public void ChatUpdated(object sender, MessageEventArgs e)
    {
        _cachedChatResponse = ChatResponse.CreateFromJSON(e.Data);
        Debug.Log(_cachedChatResponse.nickname + " : " + _cachedChatResponse.content);
        _sendChat = true;
    }

    public void Request()
    {
        if(_contentInputField.text != "")
        {
            NetworkingController networking = NetworkManager.getNetworkingController();
            RequestChat(networking);
            _contentInputField.text = "";
            _contentInputField.Select();
        }
    }

    private void RequestChat(NetworkingController networking)
    {
        _nickname = ModelManager._nickname;
        _roomId = ModelManager._roomId;
        _content = _contentInputField.text;
        _chatAt = PingManager.GameTime.ToString();

        var request = new ChatRequest(_nickname, _content, _chatAt, _roomId);
        networking.sendRequest(request);
    }

    public class ChatRequest : ClientRequest
    {
        public string nickname;
        public string content;
        public string chatAt;
        public int roomId;
        public ChatRequest(string nickname, string content, string chatAt, int roomId)
        {
            type = requestType.CHAT.ToString();
            this.nickname = nickname;
            this.content = content;
            this.chatAt = chatAt;
            this.roomId = roomId;
        }
    }

    public class ChatResponse
    {
        public string nickname;
        public string content;
        public string chatAt;
        public static ChatResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<ChatResponse>(jsonString);
        }
    }

}
