using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    public const char BlankChar = '\0';
    public int rows;
    public int cols;
    public List<string> words;
    public List<List<char>> boardCharacters;
    public List<WordPlacement> wordPlacements;
    public int difficultyIndex = -1;
    public HashSet<string> foundWords = new();
    public HashSet<char> letterHintsUsed = new();

    public Dictionary<string, object> ToJson()
    {
        Dictionary<string, object> json = new()
        {
            ["rows"] = rows,
            ["cols"] = cols,
            ["words"] = words,
            ["boardCharacters"] = boardCharacters
        };

        List<object> wordPlacementsJson = new();

        foreach (var wordPlacement in wordPlacements)
        {
            Dictionary<string, object> wordPlacementJson = new Dictionary<string, object>
            {
                ["word"] = wordPlacement.word,
                ["row"] = wordPlacement.startingPosition.row,
                ["col"] = wordPlacement.startingPosition.col,
                ["h"] = wordPlacement.horizontalDirection,
                ["v"] = wordPlacement.verticalDirection
            };
            wordPlacementsJson.Add(wordPlacementJson);
        }

        json["wordPlacements"] = wordPlacementsJson;

        if (difficultyIndex != -1)
        {
            json["difficultyIndex"] = difficultyIndex;
        }

        if (foundWords.Count > 0)
        {
            json["foundWords"] = new List<string>(foundWords);
        }

        if (letterHintsUsed.Count > 0)
        {
            json["letterHintsUsed"] = new List<char>(letterHintsUsed);
        }

        return json;
    }

    public void FromJson(JSONNode json)
    {
        rows = json["rows"].AsInt;
        cols = json["cols"].AsInt;
        words = new List<string>();
        boardCharacters = new List<List<char>>();
        wordPlacements = new List<WordPlacement>();
        difficultyIndex = json.AsObject.HasKey("difficultyIndex") ? json["difficultyIndex"] : -1;
        foundWords = new HashSet<string>();
        letterHintsUsed = new HashSet<char>();

        for (int i = 0; i < json["words"].AsArray.Count; i++)
        {
            words.Add(json["words"].AsArray[i].Value);
        }

        for (int i = 0; i < json["boardCharacters"].AsArray.Count; i++)
        {
            boardCharacters.Add(new List<char>());
            for (int j = 0; j < json["boardCharacters"][i].AsArray.Count; j++)
            {
                char character = json["boardCharacters"][i][j].Value[0];
                boardCharacters[i].Add(character);
            }
        }

        for (int i = 0; i < json["wordPlacements"].AsArray.Count; i++)
        {
            JSONNode wordPlacementJson = json["wordPlacements"].AsArray[i];
            WordPlacement wordPlacement = new WordPlacement
            {
                word = wordPlacementJson["word"].Value,
                startingPosition = new CellPosition(wordPlacementJson["row"].AsInt, wordPlacementJson["col"].AsInt),
                horizontalDirection = wordPlacementJson["h"].AsInt,
                verticalDirection = wordPlacementJson["v"].AsInt,
            };
            wordPlacements.Add(wordPlacement);
        }

        for (int i = 0; i < json["foundWords"].AsArray.Count; i++)
        {
            foundWords.Add(json["foundWords"].AsArray[i].Value);
        }

        for (int i = 0; i < json["letterHintsUsed"].AsArray.Count; i++)
        {
            letterHintsUsed.Add(json["letterHintsUsed"].AsArray[i].Value[0]);
        }
    }


    public class WordPlacement
    {
        public string word;
        public CellPosition startingPosition;
        public int verticalDirection;
        public int horizontalDirection;
    }
}
public enum WordDirection
{
    Up,
    UpRight,
    Right,
    DownRight,
    Down,
    DownLeft,
    Left,
    UpLeft,
    COUNT
}