using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;
using Unity.VisualScripting;
using Cinemachine;

public class RoomCriminalController_SPY : MonoBehaviour
{
    [SerializeField] private GameObject _spy;
    [SerializeField] private CinemachineVirtualCamera _shotCamera;
    [SerializeField] private GameObject _minimap;
    private Vector3 _spyStartPos;
    private Vector3 _spyStartRot;
    private bool shotOnce = true;
    private int _prevColor = ModelManager._spyColor;
    [SerializeField] private GameObject _cop;
    // Start is called before the first frame update
    void Start()
    {
        _spyStartPos = new Vector3(ModelManager._spyPosition.x, 0, ModelManager._spyPosition.z);
        _spyStartRot = new Vector3(ModelManager._spyRotation.x, ModelManager._spyRotation.y, ModelManager._spyRotation.z);
        //초기 player 위치, 방향설정
        _spy.transform.position = _spyStartPos;
        _spy.transform.rotation = Quaternion.Euler(_spyStartRot);
        for (int i = 0; i < 4; i++)
        {
            _spy.transform.GetChild(i).gameObject.SetActive(false);
        }
        _spy.transform.GetChild(ModelManager._spyColor).gameObject.SetActive(true);
        _spy.GetComponent<Outline>().enabled = false;
        _spy.transform.SetParent(gameObject.transform);
        _shotCamera.GetComponent<CinemachineVirtualCamera>().LookAt = _spy.transform.Find("CameraRoot");
        _shotCamera.GetComponent<CinemachineVirtualCamera>().Follow = _spy.transform;
    }
    // Update is called once per frame
    void Update()
    {
        //위치 재설정
        CriminalReplace();
        if (!ModelManager._isDisconnected)
        {
            //유저 정보 할당
            _spy.GetComponent<PlayerInfo>()._roomUser = RoomInfo._roomSpy;
            //사망 시
            if (RoomInfo._roomSpy.userState == userState.DIE.ToString() && shotOnce)
            {
                StartCoroutine("CriminalShotMotion");
                shotOnce = false;
            }
            else
            {
                CriminalMove();
                ChangeCharacter();
            }
            if (RoomInfo._finishedMission == 1)
            {
                StartCoroutine("ShowOutline");
                RoomInfo._finishedMission = 0;
            }
            _spy.GetComponent<Outline>().enabled = RoomInfo._reportTarget[1];
        }
    }
    private void ChangeCharacter()
    {
        //색이 바뀌었으면
        if (_prevColor != RoomInfo._roomSpy.color)
        {
            _spy.transform.GetChild(_prevColor).gameObject.SetActive(false);
            _spy.transform.GetChild(RoomInfo._roomSpy.color).gameObject.SetActive(true);

            _spy.transform.Find("ChangeSmoke").GetComponent<ParticleSystem>().Play();
        }
        _prevColor = RoomInfo._roomSpy.color;
    }
    IEnumerator ShowOutline()
    {
        RoomInfo._reportTarget[1] = true;
        _minimap.GetComponent<MinimapController>().MapPing(_spy.transform.position, true);
        yield return new WaitForSeconds(4);
        RoomInfo._reportTarget[1] = false;
        _minimap.GetComponent<MinimapController>().MapPing(_spy.transform.position, false);
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
    private void CharacterDie()
    {
        _spy.gameObject.GetComponentInChildren<CapsuleCollider>().enabled = false;
        _spy.GetComponent<Animator>().enabled = false;
        _spy.transform.GetChild(4).gameObject.SetActive(true);
        _spy.transform.GetChild(4).GetChild(0).GetComponent<Rigidbody>().AddForce((_spy.transform.position - _cop.transform.position).normalized * 50f, ForceMode.Impulse);
        // _criminal.transform.GetChild(1).gameObject.SetActive(false);
    }
    private void CriminalMove()
    {
        //이동
        _spy.transform.position = Vector3.MoveTowards(_spy.transform.position,
            new Vector3(RoomInfo._roomSpy.pos.x, 0, RoomInfo._roomSpy.pos.z), Time.deltaTime * RoomInfo._roomSpy.velocity);
        //회전
        _spy.transform.rotation = Quaternion.Slerp(_spy.transform.rotation,
        Quaternion.Euler(RoomInfo._roomSpy.rot.x, RoomInfo._roomSpy.rot.y, RoomInfo._roomSpy.rot.z), Time.deltaTime * 10f);
        //애니메이션
        switch (RoomInfo._roomSpy.anim)
        {
            case 1:
                _spy.GetComponent<Animator>().SetBool("isWalk", true);
                _spy.GetComponent<Animator>().SetBool("isRun", false);
                _spy.GetComponent<Animator>().SetInteger("motion", 1);
                break;
            case 2:
                _spy.GetComponent<Animator>().SetBool("isWalk", true);
                _spy.GetComponent<Animator>().SetBool("isRun", true);
                _spy.GetComponent<Animator>().SetInteger("motion", 2);
                break;
            case 3:
                _spy.GetComponent<Animator>().SetBool("isWalk", false);
                _spy.GetComponent<Animator>().SetInteger("motion", 3);
                break;
            case 4:
                _spy.GetComponent<Animator>().SetBool("isWalk", false);
                _spy.GetComponent<Animator>().SetInteger("motion", 4);
                break;
            case 5:
                _spy.GetComponent<Animator>().SetBool("isWalk", false);
                _spy.GetComponent<Animator>().SetInteger("motion", 5);
                break;
            case 6:
                _spy.GetComponent<Animator>().SetBool("isWalk", false);
                _spy.GetComponent<Animator>().SetInteger("motion", 6);
                break;
            case 7:
                _spy.GetComponent<Animator>().SetBool("isWalk", false);
                _spy.GetComponent<Animator>().SetInteger("motion", 7);
                break;
            case 8:
                _spy.GetComponent<Animator>().SetBool("isWalk", false);
                _spy.GetComponent<Animator>().SetInteger("motion", 8);
                break;
            default:
                _spy.GetComponent<Animator>().SetBool("isWalk", false);
                _spy.GetComponent<Animator>().SetBool("isRun", false);
                _spy.GetComponent<Animator>().SetInteger("motion", 0);
                break;
        }
    }
    private void CriminalReplace()
    {
        if (ModelManager._isReconnected)
        {
            Vector3 pos = new Vector3(RoomInfo._roomSpy.pos.x, 0, RoomInfo._roomSpy.pos.z);
            Quaternion rot = Quaternion.Euler(RoomInfo._roomSpy.rot.x, RoomInfo._roomSpy.rot.y, RoomInfo._roomSpy.rot.z);
            _spy.transform.position = pos;
            _spy.transform.rotation = rot;
            if (_prevColor != RoomInfo._roomSpy.color)
            {
                _spy.transform.GetChild(_prevColor).gameObject.SetActive(false);
                _spy.transform.GetChild(RoomInfo._roomSpy.color).gameObject.SetActive(true);
                _prevColor = RoomInfo._roomSpy.color;
            }

            if (RoomInfo._roomSpy.userState == userState.DIE.ToString() && shotOnce)
            {
                CharacterDie();
                shotOnce = false;
            }
        }
    }
}
