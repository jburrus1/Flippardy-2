using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Game
{
    public List<Board> Boards;
    public (string Category, string Question) FinalFlippardy;

    //public Game ConstructGameFromString(string gameString)
    //{

    //}

    public static Game GenerateTestGame()
    {
        var numBoards = 1;
        var numCats = 1;
        var numQs = 1;

        var boardList = new List<Board>();
        for(var boardIndex=0; boardIndex < numBoards; boardIndex++)
        {
            var catList = new List<Category>();
            for(var catIndex=0; catIndex < numCats; catIndex++)
            {
                var qList = new List<Question>();
                for(var qIndex=0; qIndex < numQs; qIndex++)
                {
                    qList.Add(new Question
                    {
                        Value = (qIndex + 1) * 200 * (boardIndex + 1),
                        Text = $"Board {boardIndex+1} Cat {catIndex+1} Question {qIndex + 1}"
                    });
                }
                catList.Add(new Category { Name = $"Board {boardIndex + 1} Cat {catIndex + 1}", Questions = qList });
            }
            boardList.Add(new Board { Categories = catList, BaseValue = 200*(boardIndex+1)});
        }

        return new Game { Boards = boardList, FinalFlippardy = ("Final Cat", "Final Q") };
    }
}
