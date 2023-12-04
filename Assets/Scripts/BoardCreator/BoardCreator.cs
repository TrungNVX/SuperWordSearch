using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public static class BoardCreator
{
    

    private static BoardCreatorWorker boardCreatorWorker;
    private static BoardCreatorBehaviour boardCreatorBehaviour;
    private static Action<Board> onFinishCallBack;

    public static bool IsRunning => boardCreatorWorker != null;

    public static bool IsFinished => boardCreatorWorker != null && boardCreatorWorker.Stopped;

    private static void Stop()
    {
        if (boardCreatorWorker != null)
        {
            boardCreatorWorker.Stop();
            boardCreatorWorker = null;
        }

        if (boardCreatorBehaviour != null)
        {
            Object.Destroy(boardCreatorBehaviour.gameObject);
            boardCreatorBehaviour = null;
        }
    }

    public static void CreateBoard(BoardConfig boardConfig, Action<Board> callback)
    {
        Stop();
        onFinishCallBack = callback;
        boardCreatorWorker = new BoardCreatorWorker { boardConfig = boardConfig };
        boardCreatorBehaviour = new GameObject("board_creator_behaviour").AddComponent<BoardCreatorBehaviour>();
        boardCreatorWorker.StartWorker();
    }

    public static void OnBoardWorkerFinished()
    {
        Board completedBoard = boardCreatorWorker.CompletedBoard;
        if (!string.IsNullOrEmpty(boardCreatorWorker.Error))
        {
            Debug.LogError(boardCreatorWorker.Error);
        }
        Stop();
        onFinishCallBack?.Invoke(completedBoard);
    }
    
}
public class BoardConfig
{
    public int rows;
    public int cols;
    public List<string> words;
    public string randomCharacters = "abcdefghijklmnopqrstuvwxyz";

    //In milliseconds, if this is 0 then there will be no timeout,
    //the algorithm will run till it places all words of fails to place all words
    public long algoTimeOut = 2000;

    // Specifies the number of boards to generate before the algo stops and picks the one with the most words.
    // If set to 0 then it will try all possible combinations.
    public int numberSampleToGen = 0;

    public System.Random random = new();
}