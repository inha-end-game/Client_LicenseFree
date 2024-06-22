using System;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using System.Collections;
public class RoomCopController : MonoBehaviour
{
    public GameObject _cop;
    private rVector3 _copStartPos;
    private rVector3 _copStartRot;
    private string _shotTargetUserType;
    private bool _hasShot = false;
    private static readonly string _shotResponseName = responseType.SHOT.ToString();
    void Start()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.startListenOnMessage(ShotUpdated, _shotResponseName);

        _copStartPos = ModelManager._copPosition;
        _copStartRot = ModelManager._copRotation;
        //초기 위치 지정해서 RoomUser 생성후, 리스트에 넣어줌
        _cop.transform.position = new Vector3(_copStartPos.x, 0, _copStartPos.z);
        _cop.transform.rotation = Quaternion.Euler(_copStartRot.x, _copStartRot.y, _copStartRot.z);
    }
    void OnDestroy()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.stopListenOnMessage(ShotUpdated, _shotResponseName);
    }
    // Update is called once per frame
    void Update()
    {
        CriminalReplace();
        //끊기지 않았을때
        if (!ModelManager._isDisconnected)
        {
            //Cop
            //유저 정보 할당
            _cop.GetComponent<PlayerInfo>()._roomUser = RoomInfo._roomCop;
            CopMove();
        }
        _cop.GetComponent<Outline>().enabled = RoomInfo._reportTarget[4];
    }
    private void CopMove()
    {
        //이동
        _cop.transform.position = Vector3.MoveTowards(_cop.transform.position,
            new Vector3(RoomInfo._roomCop.pos.x, 0, RoomInfo._roomCop.pos.z), Time.deltaTime * RoomInfo._roomCop.velocity);
        //회전
        _cop.transform.rotation = Quaternion.Slerp(_cop.transform.rotation,
        Quaternion.Euler(RoomInfo._roomCop.rot.x, RoomInfo._roomCop.rot.y, RoomInfo._roomCop.rot.z), Time.deltaTime * 8.0f);

        switch (RoomInfo._roomCop.anim)
        {
            case 1:
                _cop.GetComponent<Animator>().SetBool("isAiming", false);
                _cop.GetComponent<Animator>().SetBool("isWalk", true);
                _cop.GetComponent<Animator>().SetBool("isRun", false);
                break;
            case 2:
                _cop.GetComponent<Animator>().SetBool("isAiming", false);
                _cop.GetComponent<Animator>().SetBool("isWalk", true);
                _cop.GetComponent<Animator>().SetBool("isRun", true);
                break;
            case 9:
                _cop.GetComponent<Animator>().SetBool("isWalk", false);
                _cop.GetComponent<Animator>().SetBool("isRun", false);
                _cop.GetComponent<Animator>().SetBool("isAiming", true);
                break;
            default:
                _cop.GetComponent<Animator>().SetBool("isAiming", false);
                _cop.GetComponent<Animator>().SetBool("isWalk", false);
                _cop.GetComponent<Animator>().SetBool("isRun", false);
                break;
        }
        if (_hasShot)
        {
            if (_shotTargetUserType == roomUserType.USER.ToString())
            {
                StartCoroutine("CriminalShotMotion");
            }
            else
            {
                _cop.GetComponent<Animator>().SetTrigger("hasShot");
            }
            _hasShot = false;
        }
    }
    IEnumerator CriminalShotMotion()
    {
        float motionTime = 0f;
        bool shotOnce = true;
        while (motionTime < 3)
        {
            motionTime += Time.deltaTime;
            if (shotOnce && motionTime > 2.5)
            {
                _cop.GetComponent<Animator>().SetTrigger("hasShot");
                shotOnce = false;
            }
            yield return new WaitForFixedUpdate();
        }
    }
    private void CriminalReplace()
    {
        if (ModelManager._isReconnected)
        {
            //위치, 방향 업데이트
            Vector3 pos = new Vector3(RoomInfo._roomCop.pos.x, 0, RoomInfo._roomCop.pos.z);
            Quaternion rot = Quaternion.Euler(RoomInfo._roomCop.rot.x, RoomInfo._roomCop.rot.y, RoomInfo._roomCop.rot.z);
            _cop.transform.position = pos;
            _cop.transform.rotation = rot;
        }
    }
    private void ShotUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        ShotResponse ShotInfo = ShotResponse.CreateFromJSON(e.Data);
        _shotTargetUserType = ShotInfo.targetUserType;
        _hasShot = true;
    }
    public class ShotResponse
    {
        public string targetUsername;
        public string targetUserType;
        public int aliveUserCount;
        public static ShotResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<ShotResponse>(jsonString);
        }
    }
}
