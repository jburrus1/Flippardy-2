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
    List<GameObject> finalAnswers;


    GameObject finalAnswerPrefab;

    //Flags
    bool revealCategoryFlag;

    bool revealQuestionFlag;

    bool startAnswersFlag;

    bool startShowcaseFlag;

    bool advanceShowcaseFlag;





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

        finalAnswerPrefab = Resources.Load<GameObject>("Prefabs/FinalAnswer");


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
        // TODO: I have an extra step here that I forgot. Host needs to start answers after reading question

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

        yield return new WaitForSeconds(5);

        for(var i=0; i<bets.Count; i++)
        {
            //var finalAnswer = Instantiate(finalAnswerPrefab);

            var bet = bets[i] < 0 ? 0 : bets[i];
            var answer = answers[i].Equals("") ? "No answer :(" : answers[i];

            //finalAnswer.transform.Find("Bet").GetComponent<Text>().text = bet.ToString();
            //finalAnswer.transform.Find("Answer").GetComponent<Text>().text = answer;
            //finalAnswer.transform.Find("Player").GetComponent<Text>().text = GameManager.Instance.PlayerList[i].Name;

            Debug.Log($"{GameManager.Instance.PlayerList[i].Name} bet {bet} and answerwed {answer}");

            //Set transform correctly

            //finalAnswers.Add(finalAnswer);
        }

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
        bets[index] = bet;
    }

    public void SubmitAnswer(string player, string answer)
    {
        var index = GameManager.Instance.PlayerList.FindIndex(x => x.Name.Equals(player));
        // todo: check for legal bet
        answers[index] = answer;
    }

    public void ProgressShowcase()
    {
        advanceShowcaseFlag = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
