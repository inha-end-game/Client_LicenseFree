using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using TMPro;

public class ErrorHandler : MonoBehaviour
{
    //public TextMeshProUGUI _errorText;
    private string _errorMsg;
    private static readonly string _responseName = responseType.ERROR.ToString();
    // Start is called before the first frame update
    void Start()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.startListenOnMessage(ErrorUpdated, _responseName);
    }
    void OnDestroy()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.stopListenOnMessage(ErrorUpdated, _responseName);
    }
    // Update is called once per frame
    void Update()
    {
        //_errorText.text = _errorMsg;
    }
    public void ErrorUpdated(object sender, MessageEventArgs e)
    {
        ErrorResponse errorInfo = ErrorResponse.CreateFromJSON(e.Data);
        _errorMsg = errorInfo.errMessage + "\n" + _errorMsg;
    }
    public class ErrorResponse
    {
        public string errMessage;
        public static ErrorResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<ErrorResponse>(jsonString);
        }
    }
}
