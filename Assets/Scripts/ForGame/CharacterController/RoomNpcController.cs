using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using WebSocketSharp;
using Newtonsoft.Json;
using Unity.VisualScripting;

public class RoomNpcController : MonoBehaviour
{
    public GameObject _prefab;
    private List<GameObject> _NPC = new List<GameObject>();
    private List<rVector3> _npcStartPos;
    private List<rVector3> _npcStartRot;
    [SerializeField] private GameObject _cop;
    private string _shotTarget;
    private string _killedTarget;
    private bool _hasKilled = false;
    private bool _hasShot = false;
    private static readonly string _shotResponseName = responseType.SHOT.ToString();
    private static readonly string _KillResponseName = responseType.ASSASSIN_KILL.ToString();
    // Start is called before the first frame update
    void Awake()
    {

    }
    void Start()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.startListenOnMessage(ShotUpdated, _shotResponseName);
        networking.startListenOnMessage(KilledUpdated, _KillResponseName);

        _npcStartPos = ModelManager._npcPosition;
        _npcStartRot = ModelManager._npcRotation;
        Debug.Log("NPC 수 : " + ModelManager._npcCount);
        for (int i = 0; i < ModelManager._npcCount; i++)
        {
            _NPC.Add(Instantiate(_prefab, new Vector3(_npcStartPos[i].x, 0, _npcStartPos[i].z),
                Quaternion.Euler(new Vector3(_npcStartRot[i].x, _npcStartRot[i].y, _npcStartRot[i].z))));
            for (int j = 0; j < 4; j++)
            {
                _NPC[i].transform.GetChild(j).gameObject.SetActive(false);
            }
            _NPC[i].transform.GetChild(ModelManager._npcColor[i]).gameObject.SetActive(true);
            _NPC[i].GetComponent<Outline>().enabled = false;
            _NPC[i].transform.SetParent(gameObject.transform);
        }
    }
    void OnDestroy()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.stopListenOnMessage(ShotUpdated, _shotResponseName);
        networking.stopListenOnMessage(KilledUpdated, _KillResponseName);
    }
    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _NPC.Count; i++)
        {
            //위치 재설정
            NPCReplace(i);
            if (!ModelManager._isDisconnected)
            {
                //유저 정보 할당
                _NPC[i].GetComponent<PlayerInfo>()._roomUser = RoomInfo._roomNPCs[i];
                //사망 시
                if (_hasShot && RoomInfo._roomNPCs[i].username == _shotTarget)
                {
                    CharacterArrest(i);
                    _hasShot = false;
                }
                else if (_hasKilled && RoomInfo._roomNPCs[i].username == _killedTarget)
                {
                    StartCoroutine("CharacterDie", i);
                    _hasKilled = false;
                }
                else
                {
                    NPCMove(i);
                    if (ModelManager._crimeType == crimeType.ASSASSIN.ToString() && ModelManager._missionPhase == 3)
                    {
                        //암살자일 경우 타겟들 외각선 적용
                        if (ModelManager._targetID.Contains(_NPC[i].GetComponent<PlayerInfo>()._roomUser.username))
                        {
                            _NPC[i].gameObject.GetComponent<Outline>().enabled = true;
                        }
                    }
                }
            }
        }
    }
    //경찰에게 검거시
    private void CharacterArrest(int i)
    {
        _NPC[i].GetComponent<Animator>().enabled = false;
        _NPC[i].transform.GetChild(4).gameObject.SetActive(true);
        _NPC[i].gameObject.GetComponentInChildren<CapsuleCollider>().enabled = false;
        _NPC[i].transform.GetChild(4).GetChild(0).GetComponent<Rigidbody>().AddForce((_NPC[i].transform.position - _cop.transform.position).normalized * 50f, ForceMode.Impulse);
        _NPC[i].gameObject.GetComponentInChildren<Outline>().enabled = false;
        //_NPC[i].transform.GetChild(1).gameObject.SetActive(false);
    }
    //암살자에게 암살시
    IEnumerator CharacterDie(int i)
    {
        ModelManager._targetID.Remove(_NPC[i].GetComponent<PlayerInfo>()._roomUser.username);
        _NPC[i].gameObject.GetComponentInChildren<Outline>().enabled = false;
        float motionTime = 0f;
        while (motionTime < 1)
        {
            motionTime += Time.deltaTime;
            _NPC[i].GetComponent<Animator>().SetBool("isWalk", false);
            yield return new WaitForFixedUpdate();
        }
        _NPC[i].GetComponent<Animator>().enabled = false;
        _NPC[i].transform.GetChild(4).gameObject.SetActive(true);
        _NPC[i].gameObject.GetComponentInChildren<CapsuleCollider>().enabled = false;
        //_NPC[i].transform.GetChild(4).gameObject.SetActive(false);
    }
    private void NPCMove(int i)
    {
        //이동
        _NPC[i].transform.position = Vector3.MoveTowards(_NPC[i].transform.position,
            new Vector3(RoomInfo._roomNPCs[i].pos.x, 0, RoomInfo._roomNPCs[i].pos.z), Time.deltaTime * 1.5f);
        //회전
        _NPC[i].transform.rotation = Quaternion.Slerp(_NPC[i].transform.rotation,
        Quaternion.Euler(RoomInfo._roomNPCs[i].rot.x, RoomInfo._roomNPCs[i].rot.y + 45, RoomInfo._roomNPCs[i].rot.z), Time.deltaTime * 8.0f);
        //애니메이션
        switch (RoomInfo._roomNPCs[i].anim)
        {
            case 1:
                _NPC[i].GetComponent<Animator>().SetBool("isWalk", true);
                _NPC[i].GetComponent<Animator>().SetInteger("motion", 1);
                break;
            case 3:
                _NPC[i].GetComponent<Animator>().SetBool("isWalk", false);
                _NPC[i].GetComponent<Animator>().SetInteger("motion", 3);
                break;
            case 4:
                _NPC[i].GetComponent<Animator>().SetBool("isWalk", false);
                _NPC[i].GetComponent<Animator>().SetInteger("motion", 4);
                break;
            case 5:
                _NPC[i].GetComponent<Animator>().SetBool("isWalk", false);
                _NPC[i].GetComponent<Animator>().SetInteger("motion", 5);
                break;
            case 6:
                _NPC[i].GetComponent<Animator>().SetBool("isWalk", false);
                _NPC[i].GetComponent<Animator>().SetInteger("motion", 6);
                break;
            case 7:
                _NPC[i].GetComponent<Animator>().SetBool("isWalk", false);
                _NPC[i].GetComponent<Animator>().SetInteger("motion", 7);
                break;
            default:
                _NPC[i].GetComponent<Animator>().SetBool("isWalk", false);
                _NPC[i].GetComponent<Animator>().SetInteger("motion", 0);
                break;
        }
    }
    private void NPCReplace(int i)
    {
        if (ModelManager._isReconnected)
        {
            _NPC[i].transform.position = new Vector3(RoomInfo._roomNPCs[i].pos.x, 0, RoomInfo._roomNPCs[i].pos.z);
            //회전
            _NPC[i].transform.rotation = Quaternion.Euler(RoomInfo._roomNPCs[i].rot.x, RoomInfo._roomNPCs[i].rot.y + 45, RoomInfo._roomNPCs[i].rot.z);
            //그사이에 죽었을경우
            if (RoomInfo._roomNPCs[i].userState == userState.DIE.ToString())
            {
                _NPC[i].gameObject.GetComponentInChildren<Outline>().enabled = false;
                _NPC[i].GetComponent<Animator>().enabled = false;
                _NPC[i].transform.GetChild(4).gameObject.SetActive(true);
                _NPC[i].gameObject.GetComponentInChildren<CapsuleCollider>().enabled = false;
            }
        }
    }
    private void ShotUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        ShotResponse ShotInfo = ShotResponse.CreateFromJSON(e.Data);
        _shotTarget = ShotInfo.targetUsername;
        _hasShot = true;
    }
    private void KilledUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        AssassinKillResponse KilledInfo = AssassinKillResponse.CreateFromJSON(e.Data);
        _killedTarget = KilledInfo.targetUsername;
        _hasKilled = true;
    }
    public class AssassinKillResponse
    {
        public string targetUsername;
        public static AssassinKillResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<AssassinKillResponse>(jsonString);
        }
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
