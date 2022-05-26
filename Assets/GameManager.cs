using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    List<PlayerInfo> playerList;
    string host = "";
    object playerListLock = new object();
    object hostLock = new object();


    public List<PlayerInfo> PlayerList => playerList;
    public string Host => host;
    void Start()
    {
        Instance = this;
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
                debugString += $"{player.Name} :${player.Money}";
            }
            Debug.Log(debugString);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            var game = Game.GenerateTestGame();
            var json = JsonConvert.SerializeObject(game,Formatting.Indented);
            File.WriteAllText("C:\\Temp\\Foo.json", json);
        }
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
}
