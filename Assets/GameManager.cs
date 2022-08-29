using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.IO;
using FlippardyExceptions;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private Game activeGame;

    private int boardIndex = 0;
    private bool initialized = false;

    List<PlayerInfo> playerList;
    string host = "";
    object playerListLock = new object();
    object hostLock = new object();

    private bool debugMode = false;

    private bool startGameFlag;

    private string roomCode = "";


    public List<PlayerInfo> PlayerList => playerList;
    public string Host => host;
    public bool DebugMode => debugMode;
    public Game ActiveGame => activeGame;
    public string RoomCode
    {
        get
        {
            return roomCode;
        }
        set
        {
            roomCode = value;
        }
    }
    public int BoardIndex
    {
        get
        {
            return boardIndex;
        }
        set
        {
            boardIndex = value;
        }
    }
    void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        playerList = new List<PlayerInfo>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var debugString = "";
            debugString += $"Host: {host}\nPlayers:\n";
            foreach (var player in playerList)
            {
                debugString += $"{player.Name} :${player.Money}\n";
            }
            Debug.Log(debugString);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            var game = Game.GenerateTestGame();
            var json = JsonConvert.SerializeObject(game,Formatting.Indented);
            File.WriteAllText("C:\\Temp\\Foo.json", json);
        }

        if (startGameFlag)
        {
            startGameFlag = false;
            StartCoroutine(GameBeginRoutine());
        }
    }

    private IEnumerator GameBeginRoutine()
    {
        if (debugMode)
        {
            DebugInit();
        }
        else
        {
            var json = File.ReadAllText("C:\\Temp\\Foo.json");
            activeGame = JsonConvert.DeserializeObject<Game>(json);
        }
        initialized = true;
        SceneManager.LoadScene("Board");
        yield return null;
    }

    public void DebugInit()
    {
        for(var i=0; i<3; i++)
        {
            playerList.Add(new PlayerInfo($"Player {i + 1}"));
        }
        host = "Debug Host";
        activeGame = Game.GenerateTestGame();
        boardIndex = 0;
    }

    public bool AddPlayer(string name)
    {
        lock (playerListLock)
        {
            if (playerList.Any(x => x.Name.Equals(name))){
                return false;
            }
            else
            {
                playerList.Add(new PlayerInfo(name));
                return true;
            }
        }
    }

    public bool SetHost(string name)
    {
        Debug.Log("trying to set host");
        lock (hostLock)
        {
            Debug.Log("Host lock acquired");
            if (host.Equals("") || host.Equals(name))
            {
                host = name;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public void StartDebugGame()
    {
        debugMode = true;
        StartGame();
    }

    public void StartGame()
    {
        startGameFlag = true;
        Debug.Log("Starting game");
    }

    public PlayerInfo GetWinner()
    {
        var maxMoney = int.MinValue;
        var winner = PlayerList[0];
        foreach(var player in PlayerList)
        {
            if(player.Money > maxMoney)
            {
                maxMoney = player.Money;
                winner = player;
            }
        }
        return winner;
    }
}
