using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FinalManager : MonoBehaviour
{
    public static FinalManager Instance;

    GameObject welcome;
    GameObject category;
    GameObject question;
    List<GameObject> FinalShowcaseElements;


    GameObject finalAnswerPrefab;

    GameObject finalShowcase;

    //Flags
    bool revealCategoryFlag;

    bool revealQuestionFlag;

    bool startAnswersFlag;

    bool startShowcaseFlag;

    bool advanceShowcaseFlag;


    int changeToMoney = 0;
    int currentBet = 0;


    List<int> bets;
    List<string> answers;



    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        var final = GameManager.Instance.ActiveGame.FinalFlippardy;
        bets = new List<int>();
        answers = new List<string>();

        for(var i=0; i < GameManager.Instance.PlayerList.Count; i++)
        {
            bets.Add(-1);
            answers.Add("");
        }

        welcome = transform.Find("Welcome").gameObject;
        category = transform.Find("Category").gameObject;
        question = transform.Find("Question").gameObject;

        changeToMoney = 0;
        currentBet = 0;
        finalShowcase = transform.Find("FinalShowcase").gameObject;

        finalAnswerPrefab = Resources.Load<GameObject>("Prefabs/FinalShowcaseElement");
        FinalShowcaseElements = new List<GameObject>();


        category.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = final.Category;
        question.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = final.Question;

        StartCoroutine(HandleFinal());
    }

    IEnumerator HandleFinal()
    {
        WSManager.Instance.BeginFinalFlippardy();
        var waitingForCat = true;
        while (waitingForCat)
        {
            if (revealCategoryFlag)
            {
                revealCategoryFlag = false;
                waitingForCat = false;
                welcome.SetActive(false);
            }
            yield return null;
        }
        WSManager.Instance.StartBetting();

        var waitingForBets = true;
        while (waitingForBets)
        {
            var found = false;
            foreach(var bet in bets)
            {
                if (bet.Equals(-1))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                waitingForBets = false;
            }
            yield return null;
        }

        WSManager.Instance.AllowQuestionReveal();

        var waitingForQuestion = true;
        while (waitingForQuestion)
        {
            if (revealQuestionFlag)
            {
                revealQuestionFlag = false;
                waitingForQuestion = false;
                category.SetActive(false);
            }
            yield return null;
        }

        WSManager.Instance.AllowAnswerStart();

        var waitingForAnswerStart = true;
        while (waitingForAnswerStart)
        {
            if (startAnswersFlag)
            {
                startAnswersFlag = false;
                waitingForAnswerStart = false;
            }
            yield return null;
        }

        WSManager.Instance.StartAnswer();

        yield return new WaitForSeconds(10);

        for(var i=0; i<bets.Count; i++)
        {
            var finalAnswer = Instantiate(finalAnswerPrefab);

            var bet = bets[i] < 0 ? 0 : bets[i];
            var answer = answers[i].Equals("") ? "No answer :(" : answers[i];

            finalAnswer.transform.SetParent(finalShowcase.transform);

            finalAnswer.transform.Find("Bet").transform.GetComponentInChildren<TextMeshProUGUI>().text = bet.ToString();
            finalAnswer.transform.Find("Answer").GetComponentInChildren<TextMeshProUGUI>().text = answer;
            finalAnswer.transform.Find("Player").GetComponentInChildren<TextMeshProUGUI>().text = GameManager.Instance.PlayerList[i].Name;
            finalAnswer.transform.localPosition = new Vector3(i * 1920, 0);

            Debug.Log($"{GameManager.Instance.PlayerList[i].Name} bet {bet} and answerwed {answer}");

            FinalShowcaseElements.Add(finalAnswer);
        }
        yield return Fade(question);

        for (var i=0; i<FinalShowcaseElements.Count; i++)
        {
            WSManager.Instance.AllowProgress();

            //Show Answer
            while (!advanceShowcaseFlag)
            {
                yield return null;
            }
            advanceShowcaseFlag = false;

            WSManager.Instance.StopProgress();
            yield return Fade(FinalShowcaseElements[i].transform.Find("Player").gameObject);
            WSManager.Instance.AllowProgressDecision();

            while (!advanceShowcaseFlag)
            {
                yield return null;
            }
            advanceShowcaseFlag = false;

            WSManager.Instance.StopProgress();
            yield return Fade(FinalShowcaseElements[i].transform.Find("Answer").gameObject);
            WSManager.Instance.AllowProgress();

            while (!advanceShowcaseFlag)
            {
                yield return null;
            }
            advanceShowcaseFlag = false;

            WSManager.Instance.StopProgress();
            GameManager.Instance.PlayerList[i].AddMoney(changeToMoney);
            yield return new WaitForSeconds(1);
            WSManager.Instance.SetPlayerInfo();

            advanceShowcaseFlag = false;

            if (i != FinalShowcaseElements.Count - 1)
            {
                yield return MoveToNextPlayer();
            }

        }

        //TODO: show winner
        var winner = GameManager.Instance.GetWinner();
        Debug.Log($"{winner.Name} win with {winner.Money}");
        yield return Fade(FinalShowcaseElements[FinalShowcaseElements.Count - 1].transform.Find("Bet").gameObject);

        yield return null;
    }

    public void RevealCategory()
    {
        revealCategoryFlag = true;
    }

    public void RevealQuestion()
    {
        revealQuestionFlag = true;
    }

    public void StartAnswers()
    {
        startAnswersFlag = true;
    }

    public void SubmitBet(string player, int bet)
    {
        var index = GameManager.Instance.PlayerList.FindIndex(x => x.Name.Equals(player));
        // todo: check for legal bet

        if(bet <= GameManager.Instance.PlayerList[index].Money)
        {
            bets[index] = bet;
        }
    }

    public void SubmitAnswer(string player, string answer)
    {
        var index = GameManager.Instance.PlayerList.FindIndex(x => x.Name.Equals(player));
        Debug.Log($"{player} answered {answer}");
        answers[index] = answer;
    }

    public void ProgressShowcase()
    {
        advanceShowcaseFlag = true;
    }
    public void ProgressShowcase_Correct()
    {
        changeToMoney = currentBet;
        advanceShowcaseFlag = true;
    }
    public void ProgressShowcase_Incorrect()
    {
        changeToMoney = -currentBet;
        advanceShowcaseFlag = true;
    }

    private IEnumerator MoveToNextPlayer()
    {
        var startValue = finalShowcase.transform.position;
        var endValue = finalShowcase.transform.position - new Vector3(1920, 0);

        var timeElapsed = 0f;
        var lerpDuration = 1f;
        while (timeElapsed < lerpDuration)
        {
            finalShowcase.transform.position = Vector3.Lerp(startValue, endValue, timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        finalShowcase.transform.position = endValue;
    }

    private IEnumerator Fade(GameObject objToFade)
    {
        var imageObj = objToFade.GetComponent<Image>();
        var textObj = objToFade.GetComponentInChildren<TextMeshProUGUI>();

        var imageColor = imageObj.color;
        var textColor = textObj.color;

        var imageStart = imageObj.color.a;
        var textStart = textObj.color.a;
        var endValue = 0;

        var timeElapsed = 0f;
        var lerpDuration = 1f;
        while (timeElapsed < lerpDuration)
        {
            var newImageA = Mathf.Lerp(imageStart, endValue, timeElapsed / lerpDuration);
            var newTextA = Mathf.Lerp(textStart, endValue, timeElapsed / lerpDuration);

            imageObj.color = new Color(imageColor.r, imageColor.g, imageColor.b, newImageA);
            textObj.color = new Color(textColor.r, textColor.g, textColor.b, newTextA);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        imageObj.color = new Color(imageColor.r, imageColor.g, imageColor.b, 0);
        textObj.color = new Color(textColor.r, textColor.g, textColor.b, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
