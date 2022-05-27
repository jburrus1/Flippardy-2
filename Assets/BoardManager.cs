using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    const float width = 1920;
    const float height = 1080;

    int numCats;
    int numQs;

    GameObject gridElementPrefab;
    GameObject questionElementPrefab;

    // organized by cat, q
    List<List<GameObject>> questions;

    Transform gridTransform;
    Transform playerTransform;
    GameObject activeQuestionDisplay;


    //Flags
    bool hostSelectFlag = false;
    Vector2Int hostSelectedQ;

    bool hostActivateQFlag = false;

    bool buzzInFlag = false;
    string buzzInName = "";

    bool hostDecisionFlag = false;
    bool hostDecision = false;

    bool hostCancelQFlag = false;



    object buzzInLock = new object();


    float gridCellHeight;
    float gridCellWidth;

    Board board;

    public Board Board => board;
    public List<List<GameObject>> Questions => questions;
    void Start()
    {
        Instance = this;
        board = GameManager.Instance.ActiveGame.Boards[GameManager.Instance.BoardIndex];
        gridElementPrefab = Resources.Load<GameObject>("Prefabs/GridElement");
        questionElementPrefab = Resources.Load<GameObject>("Prefabs/QuestionElement");

        gridTransform = gameObject.transform.Find("Grid").transform;
        playerTransform = gameObject.transform.Find("Players").transform;
        questions = new List<List<GameObject>>();
        InitializeBoard();

        StartCoroutine(HandleBoard());
    }

    private void InitializeBoard()
    {
        // Assuming that all categories have the same number of questions
        numQs = board.Categories[0].Questions.Count;
        numCats = board.Categories.Count;

        // Divided heightwise by all questions, category name, and player list
        gridCellHeight = height / (numQs + 2);

        // Divided widthwise by all categories
        gridCellWidth = width / (numCats);

        // Divided wwidthwise by all players;
        var playerCellWidth = width / GameManager.Instance.PlayerList.Count;

        var catIndex = 0;
        foreach(var category in board.Categories)
        {
            var qIndex = 0;
            var catObj = Instantiate(gridElementPrefab);
            catObj.transform.SetParent(gridTransform);
            var catRect = catObj.GetComponent<RectTransform>();
            catRect.anchoredPosition = new Vector3(catIndex * gridCellWidth, 0);
            catRect.sizeDelta = new Vector2(gridCellWidth, gridCellHeight);
            catObj.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().SetText(category.Name);
            questions.Add(new List<GameObject>());
            foreach(var question in category.Questions)
            {
                var qObj = Instantiate(gridElementPrefab);
                qObj.transform.SetParent(gridTransform);
                var qRect = qObj.GetComponent<RectTransform>();
                qRect.anchoredPosition = new Vector3(catIndex * gridCellWidth, -(gridCellHeight * (qIndex+1)));
                qRect.sizeDelta = new Vector2(gridCellWidth, gridCellHeight);
                qIndex++;
                qObj.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().SetText($"${question.Value}");
                questions.Last().Add(qObj);
            }
            catIndex++;
        }

        var playerIndex = 0;
        foreach(var player in GameManager.Instance.PlayerList)
        {
            var playerObj = Instantiate(gridElementPrefab);
            playerObj.transform.SetParent(gridTransform);
            var playerRect = playerObj.GetComponent<RectTransform>();
            playerRect.anchoredPosition = new Vector3(playerIndex * playerCellWidth, -(gridCellHeight*(numQs + 1)));
            playerRect.sizeDelta = new Vector2(playerCellWidth, gridCellHeight);
            playerIndex++;
            playerObj.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().SetText($"{player.Name}\n${player.Money}");
        }


    }

    private IEnumerator HandleQuestion(int catIndex, int qIndex)
    {
        if(questions[catIndex][qIndex] is null)
        {
            yield return null;
        }
        else
        {
            foreach(var player in GameManager.Instance.PlayerList)
            {
                player.SetCanAnswer(true);
            }
            var question = board.Categories[catIndex].Questions[qIndex];
            var qObj = Instantiate(questionElementPrefab);
            activeQuestionDisplay = qObj;
            qObj.transform.SetParent(gameObject.transform);
            var playerRect = qObj.GetComponent<RectTransform>();
            playerRect.anchoredPosition = new Vector3(catIndex * gridCellWidth, -(gridCellHeight * (qIndex+1)));
            playerRect.sizeDelta = new Vector2(width, height);
            qObj.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().SetText($"{question.Text}");
            playerRect.localScale = new Vector2(1f / numCats, 1f / (numQs + 2));

            var originalPos = playerRect.anchoredPosition;
            var originalScale = playerRect.localScale;

            var targetPos = new Vector2(0, 0);
            var targetScale = new Vector2(1, 1);

            var lerpDuration = 0.5f;
            var timeElapsed = 0f;


            while (timeElapsed < lerpDuration)
            {
                playerRect.anchoredPosition = Vector2.Lerp(originalPos, targetPos, timeElapsed / lerpDuration);
                playerRect.localScale = Vector2.Lerp(originalScale, targetScale, timeElapsed / lerpDuration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            playerRect.anchoredPosition = targetPos;
            playerRect.localScale = targetScale;

            WSManager.Instance.ActivateSpeakMode();


            var isWaitingForHost = true;

            while (isWaitingForHost)
            {
                if (Input.GetMouseButtonDown(0) && GameManager.Instance.DebugMode)
                {
                    isWaitingForHost = false;
                }
                if (hostActivateQFlag)
                {
                    isWaitingForHost = false;
                    hostActivateQFlag = false;
                    WSManager.Instance.SetPlayerInfo();
                }
                yield return null;
            }

            if (!GameManager.Instance.DebugMode)
            {
                var waitingForPlayers = true;
                while (waitingForPlayers)
                {
                    if (buzzInFlag)
                    {
                        buzzInFlag = false;

                        WSManager.Instance.DisableBuzzers();
                        WSManager.Instance.ActivateDecisionMode();

                        var waitingForHostDecision = true;
                        while (waitingForHostDecision)
                        {
                            if (hostDecisionFlag)
                            {
                                hostDecisionFlag = false;
                                waitingForHostDecision = false;
                                if (hostDecision)
                                {
                                    AwardPlayer(buzzInName, question.Value);
                                    waitingForPlayers = false;
                                }
                                else
                                {
                                    AwardPlayer(buzzInName, -question.Value);
                                }
                                WSManager.Instance.SetPlayerInfo();
                            }
                            yield return null;
                        }
                    }
                    if (hostCancelQFlag)
                    {
                        hostCancelQFlag = false;
                        waitingForPlayers = false;
                    }

                    yield return null;
                }
            }


            RemoveQuestion(catIndex, qIndex);

            WSManager.Instance.ActivateQuesitonSelectMode();


        }
        yield return null;
    }

    private void AwardPlayer(string playerName, int value)
    {
        var player = GameManager.Instance.PlayerList.First(x => x.Name.Equals(playerName));
        player.AddMoney(value);
        player.SetCanAnswer(false);
    }

    private void RemoveQuestion(int cat, int q)
    {
        var questionObj = questions[cat][q];
        questions[cat][q] = null;
        Destroy(questionObj);
        Destroy(activeQuestionDisplay);
    }

    private IEnumerator HandleBoard()
    {
        var allQuestionsDone = false;

        while (!allQuestionsDone)
        {
            allQuestionsDone = true;
            foreach (var quesiton in questions.SelectMany(x=>x))
            {
                if(!(quesiton is null))
                {
                    allQuestionsDone = false;
                    break;
                }
            }
            if (Input.GetMouseButtonDown(0) && GameManager.Instance.DebugMode)
            {
                var gridX = Mathf.FloorToInt(Input.mousePosition.x / gridCellWidth);
                var gridY = board.Categories[0].Questions.Count + 2 - Mathf.CeilToInt(Input.mousePosition.y / gridCellHeight);

                if ((gridY > 0) && (gridY < board.Categories[0].Questions.Count + 1))
                {
                    Debug.Log($"{gridX},{gridY - 1}");
                    yield return HandleQuestion(gridX, gridY - 1);
                }
            }
            //TODO: include support for player input
            if (hostSelectFlag)
            {
                hostSelectFlag = false;
                yield return HandleQuestion(hostSelectedQ.x,hostSelectedQ.y);
            }
            yield return null;
        }

        // When it gets here, all questions are done

        GameManager.Instance.BoardIndex = GameManager.Instance.BoardIndex + 1;

        if(GameManager.Instance.BoardIndex < GameManager.Instance.ActiveGame.Boards.Count)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        yield return null;
    }

    public void SelectQuestion(int cat, int q)
    {
        hostSelectedQ = new Vector2Int(cat, q);
        hostSelectFlag = true;

        Debug.Log($"Host selected {cat},{q}");
    }

    public void ActivateQuestion()
    {
        hostActivateQFlag = true;
    }

    public void CancelQuestion()
    {
        hostCancelQFlag = true;
    }

    public void BuzzIn(string playerName)
    {
        lock (buzzInLock)
        {
            buzzInFlag = true;
            buzzInName = playerName;
        }
    }

    public void HostDecision(bool decision)
    {
        hostDecisionFlag = true;
        hostDecision = decision;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
