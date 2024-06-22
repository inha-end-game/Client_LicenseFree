using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;
using Unity.VisualScripting;
using System;
using Cinemachine;

public class RoomCriminalController_ASSASSIN : MonoBehaviour
{
    [SerializeField] private GameObject _assassin;
    [SerializeField] private CinemachineVirtualCamera _shotCamera;
    [SerializeField] private GameObject _minimap;
    private Vector3 _assassinStartPos;
    private Vector3 _assassinStartRot;
    private bool shotOnce = true;
    private int _prevColor = ModelManager._assassinColor;
    [SerializeField] private GameObject _cop;
    // Start is called before the first frame update
    void Start()
    {
        _assassinStartPos = new Vector3(ModelManager._assassinPosition.x, 0, ModelManager._assassinPosition.z);
        _assassinStartRot = new Vector3(ModelManager._assassinRotation.x, ModelManager._assassinRotation.y, ModelManager._assassinRotation.z);
        //초기 player 위치, 방향설정
        _assassin.transform.position = _assassinStartPos;
        _assassin.transform.rotation = Quaternion.Euler(_assassinStartRot);
        for (int i = 0; i < 4; i++)
        {
            _assassin.transform.GetChild(i).gameObject.SetActive(false);
        }
        _assassin.transform.GetChild(ModelManager._assassinColor).gameObject.SetActive(true);
        _assassin.GetComponent<Outline>().enabled = false;
        _assassin.transform.SetParent(gameObject.transform);
        _shotCamera.GetComponent<CinemachineVirtualCamera>().LookAt = _assassin.transform.Find("CameraRoot");
        _shotCamera.GetComponent<CinemachineVirtualCamera>().Follow = _assassin.transform;
    }
    // Update is called once per frame
    void Update()
    {
        //위치 재설정
        CriminalReplace();
        if (!ModelManager._isDisconnected)
        {
            //유저 정보 할당
            _assassin.GetComponent<PlayerInfo>()._roomUser = RoomInfo._roomAssassin;
            //사망 시
            if (RoomInfo._roomAssassin.userState == userState.DIE.ToString() && shotOnce)
            {
                StartCoroutine("CriminalShotMotion");
                shotOnce = false;
            }
            else
            {
                CriminalMove();
                ChangeCharacter();
            }
            if (RoomInfo._finishedMission == 3)
            {
                StartCoroutine("ShowOutline");
                RoomInfo._finishedMission = 0;
            }
            _assassin.GetComponent<Outline>().enabled = RoomInfo._reportTarget[3];
        }
    }
    private void ChangeCharacter()
    {
        //색이 바뀌었으면
        if (_prevColor != RoomInfo._roomAssassin.color)
        {
            _assassin.transform.GetChild(_prevColor).gameObject.SetActive(false);
            _assassin.transform.GetChild(RoomInfo._roomAssassin.color).gameObject.SetActive(true);

            _assassin.transform.Find("ChangeSmoke").GetComponent<ParticleSystem>().Play();
        }
        _prevColor = RoomInfo._roomAssassin.color;
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
        RoomInfo._reportTarget[3] = true;
        _minimap.GetComponent<MinimapController>().MapPing(_assassin.transform.position, true);
        yield return new WaitForSeconds(4);
        RoomInfo._reportTarget[3] = false;
        _minimap.GetComponent<MinimapController>().MapPing(_assassin.transform.position, false);
    }
    private void CharacterDie()
    {
        _assassin.gameObject.GetComponentInChildren<CapsuleCollider>().enabled = false;
        _assassin.GetComponent<Animator>().enabled = false;
        _assassin.transform.GetChild(4).gameObject.SetActive(true);
        _assassin.transform.GetChild(4).GetChild(0).GetComponent<Rigidbody>().AddForce((_assassin.transform.position - _cop.transform.position).normalized * 50f, ForceMode.Impulse);
        // _criminal.transform.GetChild(1).gameObject.SetActive(false);
    }
    private void CriminalMove()
    {
        //이동
        _assassin.transform.position = Vector3.MoveTowards(_assassin.transform.position,
            new Vector3(RoomInfo._roomAssassin.pos.x, 0, RoomInfo._roomAssassin.pos.z), Time.deltaTime * RoomInfo._roomAssassin.velocity);
        //회전
        _assassin.transform.rotation = Quaternion.Slerp(_assassin.transform.rotation,
        Quaternion.Euler(RoomInfo._roomAssassin.rot.x, RoomInfo._roomAssassin.rot.y, RoomInfo._roomAssassin.rot.z), Time.deltaTime * 10f);
        //애니메이션
        switch (RoomInfo._roomAssassin.anim)
        {
            case 1:
                _assassin.GetComponent<Animator>().SetBool("isWalk", true);
                _assassin.GetComponent<Animator>().SetBool("isRun", false);
                _assassin.GetComponent<Animator>().SetInteger("motion", 1);
                break;
            case 2:
                _assassin.GetComponent<Animator>().SetBool("isWalk", true);
                _assassin.GetComponent<Animator>().SetBool("isRun", true);
                _assassin.GetComponent<Animator>().SetInteger("motion", 2);
                break;
            case 3:
                _assassin.GetComponent<Animator>().SetBool("isWalk", false);
                _assassin.GetComponent<Animator>().SetInteger("motion", 3);
                break;
            case 4:
                _assassin.GetComponent<Animator>().SetBool("isWalk", false);
                _assassin.GetComponent<Animator>().SetInteger("motion", 4);
                break;
            case 5:
                _assassin.GetComponent<Animator>().SetBool("isWalk", false);
                _assassin.GetComponent<Animator>().SetInteger("motion", 5);
                break;
            case 6:
                _assassin.GetComponent<Animator>().SetBool("isWalk", false);
                _assassin.GetComponent<Animator>().SetInteger("motion", 6);
                break;
            case 7:
                _assassin.GetComponent<Animator>().SetBool("isWalk", false);
                _assassin.GetComponent<Animator>().SetInteger("motion", 7);
                break;
            case 8:
                _assassin.GetComponent<Animator>().SetBool("isWalk", false);
                _assassin.GetComponent<Animator>().SetInteger("motion", 8);
                break;
            case 12:
                _assassin.GetComponent<Animator>().SetBool("isWalk", false);
                _assassin.GetComponent<Animator>().SetInteger("motion", 12);
                break;
            default:
                _assassin.GetComponent<Animator>().SetBool("isWalk", false);
                _assassin.GetComponent<Animator>().SetBool("isRun", false);
                _assassin.GetComponent<Animator>().SetInteger("motion", 0);
                break;
        }
    }
    private void CriminalReplace()
    {
        if (ModelManager._isReconnected)
        {
            //위치, 방향 업데이트
            Vector3 pos = new Vector3(RoomInfo._roomAssassin.pos.x, 0, RoomInfo._roomAssassin.pos.z);
            Quaternion rot = Quaternion.Euler(RoomInfo._roomAssassin.rot.x, RoomInfo._roomAssassin.rot.y, RoomInfo._roomAssassin.rot.z);
            _assassin.transform.position = pos;
            _assassin.transform.rotation = rot;
            //캐릭터 변경시
            if (_prevColor != RoomInfo._roomAssassin.color)
            {
                _assassin.transform.GetChild(_prevColor).gameObject.SetActive(false);
                _assassin.transform.GetChild(RoomInfo._roomAssassin.color).gameObject.SetActive(true);

                _prevColor = RoomInfo._roomAssassin.color;
            }
            //죽었을시
            if (RoomInfo._roomAssassin.userState == userState.DIE.ToString() && shotOnce)
            {
                CharacterDie();
                shotOnce = false;
            }
        }
    }
}