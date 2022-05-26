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
        var game = new Game
        {
            Boards = new List<Board>
            {
                new Board
                {
                    Categories = new List<Category>
                    {
                        new Category
                        {
                            Name = "Cat1",
                            Questions = new List<Question>
                            {
                                new Question
                                {
                                    Value = 100,
                                    Text = "Q1"
                                },
                                new Question
                                {
                                    Value = 200,
                                    Text = "Q2"
                                }
                            }
                        },
                        new Category
                        {
                            Name = "Cat2",
                            Questions = new List<Question>
                            {
                                new Question
                                {
                                    Value = 100,
                                    Text = "Q1"
                                },
                                new Question
                                {
                                    Value = 200,
                                    Text = "Q2"
                                }
                            }
                        }
                    }
                },
                new Board
                {
                    Categories = new List<Category>
                    {
                        new Category
                        {
                            Name = "Cat1",
                            Questions = new List<Question>
                            {
                                new Question
                                {
                                    Value = 100,
                                    Text = "Q1"
                                },
                                new Question
                                {
                                    Value = 200,
                                    Text = "Q2"
                                }
                            }
                        },
                        new Category
                        {
                            Name = "Cat2",
                            Questions = new List<Question>
                            {
                                new Question
                                {
                                    Value = 100,
                                    Text = "Q1"
                                },
                                new Question
                                {
                                    Value = 200,
                                    Text = "Q2"
                                }
                            }
                        }
                    }
                }
            },
            FinalFlippardy = ("Final Cat", "Final Q")
        };

        return game;
    }
}
