using System;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;

public class AimController : MonoBehaviour
{
    [Header("Aim")]
    [Tooltip("laser pointer")]
    private LineRenderer _laser;
    private rVector3 _aimPos;
    private string _aimState;
    private static readonly String _responseName = responseType.AIM.ToString();
    // Start is called before the first frame update
    void Start()
    {
        _laser = GetComponent<LineRenderer>();

        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.startListenOnMessage(AimUpdated, _responseName);
    }
    void OnDestroy()
    {
        if (ModelManager._userType != roomUserType.COP.ToString())
        {
            NetworkingController networking = NetworkManager.getNetworkingController();
            networking.stopListenOnMessage(AimUpdated, _responseName);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (_aimState == aimState.START.ToString())
        {
            if (_aimPos != null)
            {
                _laser.SetPosition(0, GameObject.Find("GunHole").GetComponent<Transform>().position);
                _laser.SetPosition(1, new Vector3(_aimPos.x, _aimPos.y, _aimPos.z));
            }
        }
        else if (_aimState == aimState.PROCESS.ToString())
        {
            if (_aimPos != null)
            {
                _laser.SetPosition(0, GameObject.Find("GunHole").GetComponent<Transform>().position);
                _laser.SetPosition(1, new Vector3(_aimPos.x, _aimPos.y, _aimPos.z));
                _laser.enabled = true;
            }
        }
        else if (_aimState == aimState.END.ToString())
        {
            _laser.enabled = false;
        }
    }
    public void AimUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        AimResponse aimInfo = AimResponse.CreateFromJSON(e.Data);

        _aimPos = aimInfo.aimPos;
        _aimState = aimInfo.aimState;
    }
    public class AimResponse
    {
        public rVector3 aimPos;
        public string aimAt;
        public string aimState;
        public static AimResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<AimResponse>(jsonString);
        }
    }
}
