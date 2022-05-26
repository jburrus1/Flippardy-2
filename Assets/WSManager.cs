using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocketIOClient;
using System.Net.WebSockets;
using UnityEngine;
using TMPro;

public class WSManager : MonoBehaviour
{
    SocketIO ws;
    TextMeshProUGUI testText;
    TextMeshProUGUI playerListUI;
    TextMeshProUGUI hostNameUI;

    string roomCode;

    string playerListString = "";

    bool updatePlayerListFlag = false;
    bool updateHostFlag = false;
    // Start is called before the first frame update
    private void Start()
    {
        var canvas = GameObject.Find("Canvas");
        testText = GameObject.Find("test text").GetComponent<TextMeshProUGUI>();
        playerListUI = GameObject.Find("Player List").GetComponent<TextMeshProUGUI>();
        hostNameUI = GameObject.Find("Host Text").GetComponent<TextMeshProUGUI>();
        // Creating object of random class
        System.Random rand = new System.Random();

        // Choosing the size of string
        // Using Next() string
        int stringlen = 4;
        int randValue;
        string str = "";
        char letter;
        for (int i = 0; i < stringlen; i++)
        {

            // Generating a random number.
            randValue = rand.Next(0, 26);

            // Generating random character by converting
            // the random number into character.
            letter = Convert.ToChar(randValue + 65);

            // Appending the letter to string.
            str = str + letter;
        }

        roomCode = str;

        testText.SetText($"Room code: {roomCode}");
    }
    private async void Awake()
    {
        var url = new Uri("https://flippardy.glitch.me");
        ws = new SocketIO(url, new SocketIOOptions()
        {
            ReconnectionDelay = 100,
            ConnectionTimeout = new TimeSpan(0, 0, 10),
            Reconnection = true,
            ReconnectionAttempts = 5,
            EIO = 4,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
            ExtraHeaders = new Dictionary<string, string> { { "User-Agent", "Unity3D" } }
        }); ;
        ws.OnConnected += async (sender, e) =>
        {
            var room = $"{roomCode}_client";
            await ws.EmitAsync("client_join",room);
        };

        ws.OnReconnectAttempt += async (sender, e) =>
        {
            Debug.Log("Attempt");
        };

        ws.OnReconnectError += async (sender, e) =>
        {
            Debug.LogException(e);
        };

        ws.On("join_room", data =>
        {
            Debug.Log(data);
            var client_name = data.GetValue<string>(0);
            var room_code = data.GetValue<string>(1);
            if (GameManager.Instance.AddPlayer(client_name))
            {
                playerListString += $"{client_name}\n";
                updatePlayerListFlag = true;
            }
        });



        ws.On("join_room_host", data =>
        {
            Debug.Log(data);
            var host_name = data.GetValue<string>(0);
            var room_code = data.GetValue<string>(1);

            if (GameManager.Instance.SetHost(host_name))
            {
                Debug.Log("Host Success");
                ws.EmitAsync("join_success_host", host_name);
                updateHostFlag = true;
            }
            else
            {
                Debug.Log("Host taken");
                ws.EmitAsync("join_host_taken", host_name);
            }
        });

        ws.On("client", action =>{
            Debug.Log("Received!");
        });
        await ws.ConnectAsync();
    }

    void Update(){

        if (updatePlayerListFlag)
        {
            updatePlayerListFlag = false;
            playerListUI.SetText(playerListString);
        }
        if (updateHostFlag)
        {
            updateHostFlag = false;
            hostNameUI.SetText("Hosted by " + GameManager.Instance.Host) ;
        }
    }

}
