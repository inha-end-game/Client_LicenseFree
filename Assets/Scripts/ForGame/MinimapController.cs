using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MinimapController : MonoBehaviour
{
    [SerializeField] private GameObject _pingPrefab;
    [SerializeField] private GameObject _player;
    private List<GameObject> _ping = new List<GameObject>();
    private List<Vector3> _pingLocation = new List<Vector3>();
    private List<GameObject> _pingReport = new List<GameObject>();
    private List<Vector3> _pingLocationReport = new List<Vector3>();
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {

    }
    void LateUpdate()
    {
        UpdatePing();
    }
    /// <summary>
    /// 범죄자 미션, 경찰 범죄자미션 위치 표시
    /// </summary>
    /// <param name="eventPos"></param>
    /// <param name="show"></param>
    public void MapPing(Vector3 eventPos, bool show)
    {
        if (show)
        {
            GameObject ping = Instantiate(_pingPrefab);
            //ping.transform.SetParent(_player.transform);
            _ping.Add(ping);
            _pingLocation.Add(eventPos);
            StartCoroutine("StartPing", ping);
        }
        else
        {
            Destroy(_ping[0]);
            _ping.RemoveAt(0);
            _pingLocation.RemoveAt(0);
        }
    }
    /// <summary>
    /// 경찰 범죄자 신고 표시
    /// </summary>
    /// <param name="eventPos"></param>
    /// <param name="show"></param>
    public void MapPingReport(Vector3 eventPos, bool show)
    {
        if (show)
        {
            GameObject ping = Instantiate(_pingPrefab);
            //ping.transform.SetParent(_player.transform);
            _pingReport.Add(ping);
            _pingLocationReport.Add(eventPos);
            StartCoroutine("StartPing", ping);
        }
        else
        {
            Destroy(_pingReport[0]);
            _pingReport.RemoveAt(0);
            _pingLocationReport.RemoveAt(0);
        }
    }
    //핑 위치 업데이트
    private void UpdatePing()
    {
        for (int i = 0; i < _ping.Count; i++)
        {
            Vector3 pingPos = new Vector3();
            //미니맵 안쪽일때
            if (Vector3.Distance(_player.transform.position, _pingLocation[i]) < 14)
            {
                pingPos = _pingLocation[i];
            }
            //밖일때
            else
            {
                pingPos = _player.transform.position + ((_pingLocation[i] - _player.transform.position).normalized * 14);
            }
            _ping[i].transform.position = pingPos;
            _ping[i].transform.rotation = _player.transform.GetChild(1).transform.rotation;
        }
        for (int i = 0; i < _pingReport.Count; i++)
        {
            Vector3 pingPos = new Vector3();
            //미니맵 안쪽일때
            if (Vector3.Distance(_player.transform.position, _pingLocationReport[i]) < 14)
            {
                pingPos = _pingLocationReport[i];
            }
            //밖일때
            else
            {
                pingPos = _player.transform.position + ((_pingLocationReport[i] - _player.transform.position).normalized * 14);
            }
            _pingReport[i].transform.position = pingPos;
            _pingReport[i].transform.rotation = _player.transform.GetChild(1).transform.rotation;
        }
    }
    IEnumerator StartPing(GameObject ping)
    {
        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime;
            float scale = Mathf.Lerp(3, 1, time);
            float opacity = Mathf.Lerp(0, 1, time);
            ping.transform.localScale = new Vector3(scale, scale, scale);
            ping.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, opacity);
            yield return new WaitForFixedUpdate();
        }
    }
}
