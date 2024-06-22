using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;
using Unity.VisualScripting;
using Cinemachine;

public class RoomCriminalController_BOOMER : MonoBehaviour
{
    [SerializeField] private GameObject _boomer;
    [SerializeField] private CinemachineVirtualCamera _shotCamera;
    [SerializeField] private GameObject _minimap;
    private Vector3 _boomerStartPos;
    private Vector3 _boomerStartRot;
    private bool shotOnce = true;
    private int _prevColor = ModelManager._boomerColor;
    [SerializeField] private GameObject _cop;
    // Start is called before the first frame update
    void Start()
    {
        _boomerStartPos = new Vector3(ModelManager._boomerPosition.x, 0, ModelManager._boomerPosition.z);
        _boomerStartRot = new Vector3(ModelManager._boomerRotation.x, ModelManager._boomerRotation.y, ModelManager._boomerRotation.z);
        //초기 player 위치, 방향설정
        _boomer.transform.position = _boomerStartPos;
        _boomer.transform.rotation = Quaternion.Euler(_boomerStartRot);
        for (int i = 0; i < 4; i++)
        {
            _boomer.transform.GetChild(i).gameObject.SetActive(false);
        }
        _boomer.transform.GetChild(ModelManager._boomerColor).gameObject.SetActive(true);
        _boomer.GetComponent<Outline>().enabled = false;
        _boomer.transform.SetParent(gameObject.transform);
        _shotCamera.GetComponent<CinemachineVirtualCamera>().LookAt = _boomer.transform.Find("CameraRoot");
        _shotCamera.GetComponent<CinemachineVirtualCamera>().Follow = _boomer.transform;
    }
    // Update is called once per frame
    void Update()
    {
        //위치 재설정
        CriminalReplace();
        if (!ModelManager._isDisconnected)
        {
            //유저 정보 할당
            _boomer.GetComponent<PlayerInfo>()._roomUser = RoomInfo._roomBoomer;
            //사망 시
            if (RoomInfo._roomBoomer.userState == userState.DIE.ToString() && shotOnce)
            {
                StartCoroutine("CriminalShotMotion");
                shotOnce = false;
            }
            else
            {
                CriminalMove();
                ChangeCharacter();
            }
            if (RoomInfo._finishedMission == 2)
            {
                StartCoroutine("ShowOutline");
                RoomInfo._finishedMission = 0;
            }
            _boomer.GetComponent<Outline>().enabled = RoomInfo._reportTarget[2];
        }
    }
    private void ChangeCharacter()
    {
        //색이 바뀌었으면
        if (_prevColor != RoomInfo._roomBoomer.color)
        {
            _boomer.transform.GetChild(_prevColor).gameObject.SetActive(false);
            _boomer.transform.GetChild(RoomInfo._roomBoomer.color).gameObject.SetActive(true);

            _boomer.transform.Find("ChangeSmoke").GetComponent<ParticleSystem>().Play();
        }
        _prevColor = RoomInfo._roomBoomer.color;
    }
    IEnumerator CriminalShotMotion()
    {
        float motionTime = 0f;
        bool playOnce = true;
        while (motionTime < 3)
        {
            motionTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
            //사격당할시 오브젝트 제거, 사망모션
            if (playOnce && motionTime > 2.5)
            {
                CharacterDie();
                playOnce = false;
            }
        }
    }
    IEnumerator ShowOutline()
    {
        RoomInfo._reportTarget[2] = true;
        _minimap.GetComponent<MinimapController>().MapPing(_boomer.transform.position, true);
        yield return new WaitForSeconds(4);
        RoomInfo._reportTarget[2] = false;
        _minimap.GetComponent<MinimapController>().MapPing(_boomer.transform.position, false);
    }
    private void CharacterDie()
    {
        _boomer.gameObject.GetComponentInChildren<CapsuleCollider>().enabled = false;
        _boomer.GetComponent<Animator>().enabled = false;
        _boomer.transform.GetChild(4).gameObject.SetActive(true);
        _boomer.transform.GetChild(4).GetChild(0).GetComponent<Rigidbody>().AddForce((_boomer.transform.position - _cop.transform.position).normalized * 50f, ForceMode.Impulse);
        // _boomer.transform.GetChild(1).gameObject.SetActive(false);
    }
    private void CriminalMove()
    {
        //이동
        _boomer.transform.position = Vector3.MoveTowards(_boomer.transform.position,
            new Vector3(RoomInfo._roomBoomer.pos.x, 0, RoomInfo._roomBoomer.pos.z), Time.deltaTime * RoomInfo._roomBoomer.velocity);
        //회전
        _boomer.transform.rotation = Quaternion.Slerp(_boomer.transform.rotation,
        Quaternion.Euler(RoomInfo._roomBoomer.rot.x, RoomInfo._roomBoomer.rot.y, RoomInfo._roomBoomer.rot.z), Time.deltaTime * 10f);
        //애니메이션
        switch (RoomInfo._roomBoomer.anim)
        {
            case 1:
                _boomer.GetComponent<Animator>().SetBool("isWalk", true);
                _boomer.GetComponent<Animator>().SetBool("isRun", false);
                _boomer.GetComponent<Animator>().SetInteger("motion", 1);
                break;
            case 2:
                _boomer.GetComponent<Animator>().SetBool("isWalk", true);
                _boomer.GetComponent<Animator>().SetBool("isRun", true);
                _boomer.GetComponent<Animator>().SetInteger("motion", 2);
                break;
            case 3:
                _boomer.GetComponent<Animator>().SetBool("isWalk", false);
                _boomer.GetComponent<Animator>().SetInteger("motion", 3);
                break;
            case 4:
                _boomer.GetComponent<Animator>().SetBool("isWalk", false);
                _boomer.GetComponent<Animator>().SetInteger("motion", 4);
                break;
            case 5:
                _boomer.GetComponent<Animator>().SetBool("isWalk", false);
                _boomer.GetComponent<Animator>().SetInteger("motion", 5);
                break;
            case 6:
                _boomer.GetComponent<Animator>().SetBool("isWalk", false);
                _boomer.GetComponent<Animator>().SetInteger("motion", 6);
                break;
            case 7:
                _boomer.GetComponent<Animator>().SetBool("isWalk", false);
                _boomer.GetComponent<Animator>().SetInteger("motion", 7);
                break;
            case 8:
                _boomer.GetComponent<Animator>().SetBool("isWalk", false);
                _boomer.GetComponent<Animator>().SetInteger("motion", 8);
                break;
            default:
                _boomer.GetComponent<Animator>().SetBool("isWalk", false);
                _boomer.GetComponent<Animator>().SetBool("isRun", false);
                _boomer.GetComponent<Animator>().SetInteger("motion", 0);
                break;
        }
    }
    private void CriminalReplace()
    {
        if (ModelManager._isReconnected)
        {
            Vector3 pos = new Vector3(RoomInfo._roomBoomer.pos.x, 0, RoomInfo._roomBoomer.pos.z);
            Quaternion rot = Quaternion.Euler(RoomInfo._roomBoomer.rot.x, RoomInfo._roomBoomer.rot.y, RoomInfo._roomBoomer.rot.z);
            _boomer.transform.position = pos;
            _boomer.transform.rotation = rot;
            if (_prevColor != RoomInfo._roomBoomer.color)
            {
                _boomer.transform.GetChild(_prevColor).gameObject.SetActive(false);
                _boomer.transform.GetChild(RoomInfo._roomBoomer.color).gameObject.SetActive(true);
                _prevColor = RoomInfo._roomBoomer.color;
            }
            if (RoomInfo._roomBoomer.userState == userState.DIE.ToString() && shotOnce)
            {
                CharacterDie();
                shotOnce = false;
            }
        }
    }
}
