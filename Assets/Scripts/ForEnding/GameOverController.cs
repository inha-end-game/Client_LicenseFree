using System;
using System.Collections;
using System.Collections.Generic;
using client.Assets.Scripts.Class;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

public class GameOverController : MonoBehaviour
{
    private bool _isGameOver = false;
    private string _ending;
    private PlayableDirector _blackScreen;
    private static readonly string _responseName = responseType.GAME_OVER.ToString();
    // Start is called before the first frame update
    void Start()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.startListenOnMessage(GameOverUpdated, _responseName);

        _blackScreen = GetComponent<PlayableDirector>();
    }
    void OnDestroy()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        networking.stopListenOnMessage(GameOverUpdated, _responseName);
    }
    // Update is called once per frame
    void Update()
    {
        StartEndingScene();
    }
    /// <summary>
    /// 각 우승 연출 재생
    /// </summary>
    private void StartEndingScene()
    {
        if (_isGameOver && ModelManager._endOnce)
        {
            //ModelManager._whileConnected = false;
            if (ModelManager._gameOverInfo.job == job.COP.ToString())
            {
                if (ModelManager._gameOverInfo.overType == overType.TRUE.ToString())
                {
                    _ending = "GameOver_TrueCop";
                }
                else if (ModelManager._gameOverInfo.overType == overType.NORMAL.ToString())
                {
                    if (ModelManager._gameOverInfo.title == "안전한 거리")
                    {
                        _ending = "GameOver_NormalCop1";
                    }
                    else
                    {
                        _ending = "GameOver_NormalCop";
                    }
                }
                else if (ModelManager._gameOverInfo.overType == overType.BAD.ToString())
                {
                    _ending = "GameOver_BadCop";
                }
            }
            else if (ModelManager._gameOverInfo.job == job.SPY.ToString())
            {
                _ending = "GameOver_Spy";
            }
            else if (ModelManager._gameOverInfo.job == job.BOOMER.ToString())
            {
                _ending = "GameOver_Boomer";
            }
            else if (ModelManager._gameOverInfo.job == job.ASSASSIN.ToString())
            {
                _ending = "GameOver_Assassin";
            }
            _isGameOver = false;
            ModelManager._endOnce = false;
            EndGameScecne();
        }
    }
    public void EndGameScecne()
    {
        SceneManager.LoadScene(_ending);
    }
    public void GameOverUpdated(object sender, MessageEventArgs e)
    {
        Debug.Log("Server says: " + e.Data);
        GameOverResponse gameOverInfo = GameOverResponse.CreateFromJSON(e.Data);
        ModelManager._gameOverInfo = gameOverInfo.info;
        _isGameOver = true;
        ModelManager._whileGame = false;
        ModelManager._whileConnected = false;
        PingManager._pingUpdated = false;
    }
    public class GameOverResponse
    {
        public GameOverInfo info;
        public static GameOverResponse CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<GameOverResponse>(jsonString);
        }
    }
}
