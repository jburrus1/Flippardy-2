using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocketIOClient;
using System.Net.WebSockets;
using UnityEngine;
using TMPro;
using System.Linq;

public class WSManager : MonoBehaviour
{
    public float buzzCollectTime;
    public static WSManager Instance;

    private bool gameStarted = false;
    SocketIO ws;
    TextMeshProUGUI testText;
    TextMeshProUGUI playerListUI;
    TextMeshProUGUI hostNameUI;

    string roomCode;

    string playerListString = "";

    bool updatePlayerListFlag = false;
    bool updateHostFlag = false;

    private bool collectBuzzes = true;
    private bool collectingBuzzes = false;

    private Dictionary<string, int> buzzDelay;
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

        GameManager.Instance.RoomCode = roomCode;

        testText.SetText($"Room code: {roomCode}");
    }
    private async void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
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
            if (gameStarted)
            {
                var matchingPlayers = GameManager.Instance.PlayerList.Where(x => x.Name.Equals(client_name)).ToList();
                if (matchingPlayers.Count == 1)
                {
                    ws.EmitAsync("start_game_player", GameManager.Instance.RoomCode, matchingPlayers[0].Name, matchingPlayers[0].Money.ToString());
                    ws.EmitAsync("set_player_info", GameManager.Instance.RoomCode, matchingPlayers[0].Name, matchingPlayers[0].Money, matchingPlayers[0].CanAnswer);
                }
            }
            else if (GameManager.Instance.AddPlayer(client_name))
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

        ws.On("select_question", data =>
        {
            var cat = data.GetValue<int>(0);
            var q = data.GetValue<int>(1);

            BoardManager.Instance.SelectQuestion(cat, q);

        });

        ws.On("activate_question", data =>
        {
            Debug.Log("Activating Question");
            BoardManager.Instance.ActivateQuestion();
        });

        ws.On("cancel_question", data =>
        {
            Debug.Log("Canceling Question");
            BoardManager.Instance.CancelQuestion();
        });

        ws.On("buzz_in", data =>
        {
            var playerName = data.GetValue<string>(0);
            var delay = data.GetValue<int>(0);

            if (collectBuzzes) {
                buzzDelay.Add(playerName, delay);
                if (!collectingBuzzes)
                {
                    StartCoroutine(CollectBuzzes());
                }
            }
            Debug.Log($"{playerName} buzzed in");
            BoardManager.Instance.BuzzIn(playerName);
        });

        ws.On("host_decision", data =>
        {
            var decision = data.GetValue<bool>(0);
            Debug.Log($"Host Decided {decision}");
            BoardManager.Instance.HostDecision(decision);
        });

        ws.On("reveal_category", data =>
        {
            FinalManager.Instance.RevealCategory();
        });

        ws.On("submit_bet", data =>
        {
            var player = data.GetValue<string>(0);
            var bet = data.GetValue<int>(1);
            FinalManager.Instance.SubmitBet(player,bet);
        });

        ws.On("reveal_question", data =>
        {
            FinalManager.Instance.RevealQuestion();
        });

        ws.On("start_answers", data =>
        {
            FinalManager.Instance.StartAnswers();
        });

        ws.On("submit_answer", data =>
        {
            var player = data.GetValue<string>(0);
            var answer = data.GetValue<string>(1);
            FinalManager.Instance.SubmitAnswer(player, answer);
        });

        ws.On("progress_showcase", data =>
        {
            FinalManager.Instance.ProgressShowcase();
        });

        ws.On("progress_showcase_correct", data =>
        {
            FinalManager.Instance.ProgressShowcase_Correct();
        });

        ws.On("progress_showcase_incorrect", data =>
        {
            FinalManager.Instance.ProgressShowcase_Incorrect();
        });


        ws.On("start_game", action => {
            if(GameManager.Instance.PlayerList.Count > 0)
            {
                Debug.Log("Starting for players");
                foreach(var player in GameManager.Instance.PlayerList)
                {
                    ws.EmitAsync("start_game_player", GameManager.Instance.RoomCode,player.Name, player.Money.ToString());
                }

                gameStarted = true;
                GameManager.Instance.StartGame();
                var startBoard = GameManager.Instance.ActiveGame.Boards[0];
            }
        });
        await ws.ConnectAsync();
    }

    public void SetPlayerInfo()
    {
        Debug.Log("setting player info");
        foreach(var player in GameManager.Instance.PlayerList)
        {
            Debug.Log($"{player.Name}, {player.Money}, {player.CanAnswer}");
            ws.EmitAsync("set_player_info", GameManager.Instance.RoomCode, player.Name, player.Money, player.CanAnswer);
        }
        collectBuzzes = true;
    }

    public void DisableBuzzers()
    {
        foreach (var player in GameManager.Instance.PlayerList)
        {
            ws.EmitAsync("set_player_info", GameManager.Instance.RoomCode, player.Name, player.Money, false);
        }
    }

    public void ActivateSpeakMode()
    {
        ws.EmitAsync("speak_mode", GameManager.Instance.RoomCode);
    }

    public void ActivateQuesitonSelectMode()
    {
        var board = BoardManager.Instance.Board;
        var catArr = board.Categories.Select(x => x.Name).ToArray();
        var numCats = catArr.Length;
        var numQs = board.Categories[0].Questions.Count;
        var baseValue = board.BaseValue;

        var activeList = new List<string>();

        for(var i=0; i< numCats; i++)
        {
            for(var j=0; j< numQs; j++)
            {
                activeList.Add((!(BoardManager.Instance.Questions[i][j] is null)).ToString());
            }
        }
        ws.EmitAsync("start_game_host", GameManager.Instance.RoomCode, catArr, numCats, numQs, baseValue, activeList.ToArray());
    }

    public void ActivateDecisionMode()
    {
        ws.EmitAsync("decision_mode", GameManager.Instance.RoomCode);
    }

    public void BeginFinalFlippardy()
    {
        ws.EmitAsync("begin_final", GameManager.Instance.RoomCode);
    }

    public void StartBetting()
    {
        ws.EmitAsync("start_bet", GameManager.Instance.RoomCode);
    }

    public void AllowQuestionReveal()
    {
        ws.EmitAsync("allow_question_reveal", GameManager.Instance.RoomCode);
    }

    public void AllowAnswerStart()
    {
        ws.EmitAsync("allow_answer_start", GameManager.Instance.RoomCode);
    }

    public void StartAnswer()
    {
        ws.EmitAsync("start_answer", GameManager.Instance.RoomCode);
    }

    public void AllowProgress()
    {
        ws.EmitAsync("allow_progress", GameManager.Instance.RoomCode);
    }
    public void StopProgress()
    {
        ws.EmitAsync("stop_progress", GameManager.Instance.RoomCode);
    }
    public void AllowProgressDecision()
    {
        ws.EmitAsync("allow_progress_decision", GameManager.Instance.RoomCode);
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

    private IEnumerator CollectBuzzes()
    {
        collectingBuzzes = true;
        var time = 0f;
        while(time < buzzCollectTime)
        {
            time += Time.deltaTime;
            yield return null;
        }

        collectingBuzzes = false;
        collectBuzzes = false;
        yield return null;
    }

}
