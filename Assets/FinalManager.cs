using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalManager : MonoBehaviour
{
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


        category.transform.Find("Text").GetComponent<Text>().text = final.Category;
        question.transform.Find("Text").GetComponent<Text>().text = final.Question;

        StartCoroutine(HandleFinal());
    }

    IEnumerator HandleFinal()
    {

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

        // TODO: Remove buttons from host
        // TODO: Give players betting capabilities

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

        // TODO: Give buttons back

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

        // TODO: Give players answer capabilities

        yield return new WaitForSeconds(1);

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
