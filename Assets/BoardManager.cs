using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviour
{
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

    Vector2Int activeQuestion;
    GameObject activeQuestionDisplay;

    bool isShowingQuestion;


    float gridCellHeight;
    float gridCellWidth;

    Board board;
    void Start()
    {
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
            isShowingQuestion = true;
            activeQuestion = new Vector2Int(catIndex, qIndex);
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

            var lerpDuration = 1f;
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


        }
        yield return null;
    }

    private void RemoveQuestion()
    {
        isShowingQuestion = false;
        var cat = activeQuestion.x;
        var q = activeQuestion.y;
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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && GameManager.Instance.DebugMode && !isShowingQuestion)
        {
            var gridX = Mathf.FloorToInt(Input.mousePosition.x / gridCellWidth);
            var gridY = board.Categories[0].Questions.Count + 2 - Mathf.CeilToInt(Input.mousePosition.y / gridCellHeight);

            if ((gridY > 0) && (gridY < board.Categories[0].Questions.Count + 2))
            {
                Debug.Log($"{gridX},{gridY - 1}");
                StartCoroutine(HandleQuestion(gridX, gridY - 1));
            }
        }
        else if (Input.GetMouseButtonDown(0) && GameManager.Instance.DebugMode && isShowingQuestion)
        {
            RemoveQuestion();
        }
        
    }
}
