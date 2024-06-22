using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _winner;
    [SerializeField] private TextMeshProUGUI _description;
    [SerializeField] private TextMeshProUGUI _condition;
    private bool _isSceneEnd = false;
    // Start is called before the first frame update
    void Start()
    {
        string winnerJob = "";
        if (ModelManager._gameOverInfo.job == roomUserType.COP.ToString())
            winnerJob = "경찰";
        else if (ModelManager._gameOverInfo.job == crimeType.BOOMER.ToString())
            winnerJob = "폭파범";
        else if (ModelManager._gameOverInfo.job == crimeType.SPY.ToString())
            winnerJob = "스파이";
        else if (ModelManager._gameOverInfo.job == crimeType.ASSASSIN.ToString())
            winnerJob = "암살자";
        else
            winnerJob = "";

        _title.text = ModelManager._gameOverInfo.title;
        _winner.text = winnerJob + " ( " + ModelManager._gameOverInfo.nickname + " ) 승리";
        _description.text = ModelManager._gameOverInfo.description;
        _condition.text = ModelManager._gameOverInfo.condition;
    }
    // Update is called once per frame
    void Update()
    {
        GoLobby();
    }
    public void EndGame()
    {
        _isSceneEnd = true;
    }
    public void GoLobby()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("Lobby");
            sendRequest();
        }
    }
    public void sendRequest()
    {
        NetworkingController networking = NetworkManager.getNetworkingController();
        RequestGameOver(networking);
    }
    private void RequestGameOver(NetworkingController networking)
    {
        var request = new GameOverConfirmRequest();
        networking.sendRequest(request);
    }
    public class GameOverConfirmRequest : ClientRequest
    {
        public string username;
        public int roomId;
        public GameOverConfirmRequest()
        {
            this.username = ModelManager._username;
            this.roomId = ModelManager._roomId;
            type = requestType.GAME_OVER_CONFIRM.ToString();
        }
    }
}
