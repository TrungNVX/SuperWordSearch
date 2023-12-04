using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BoardCreatorWorker : WorkerProgress
{
    private Stopwatch timer;
    public BoardConfig boardConfig { get; set; }
    public string Error { get; private set; }
    public Board CompletedBoard { get; private set; }

    protected override void DoWork()
    {
    }

    protected override void Begin()
    {
    }

    private class WorkingBoard
    {
        public BoardConfig boardConfig;
        public List<List<Cell>> cells;
        public List<object[]> placedWords;
        public List<List<Undo>> undos;

        public WorkingBoard(BoardConfig boardConfig)
        {
            this.boardConfig = boardConfig;
        }

        public void SetLetter(int row, int col, char letter)
        {
            Cell cell = cells[row][col];
            if (cell.letter != letter)
            {
                Undo undo = new Undo
                {
                    row = row,
                    col = col,
                    undoType = Undo.UndoType.Letter
                };
                undos[^1].Add(undo);
                cell.letter = letter;
            }
        }

        public void SetCanPlaceWord(int row, int col, WordDirection direction, bool canPlace)
        {
            Cell cell = cells[row][col];
            if (cell.canPlaceWord[direction] != canPlace)
            {
                Undo undo = new Undo
                {
                    row = row,
                    col = col,
                    direction = direction,
                    undoType = Undo.UndoType.CanPlaceWord,
                    obj = cell.canPlaceWord[direction]
                };
                undos[^1].Add(undo);
                cell.canPlaceWord[direction] = canPlace;
            }
        }

        public void SetMaxWordLength(int row, int col, WordDirection direction, int maxLength)
        {
            Cell cell = cells[row][col];
            if (cell.maxWordLength[direction] != maxLength)
            {
                Undo undo = new Undo
                {
                    row = row,
                    col = col,
                    direction = direction,
                    undoType = Undo.UndoType.MaxWordLength,
                    obj = cell.maxWordLength[direction]
                };
                undos[^1].Add(undo);
                cell.maxWordLength[direction] = maxLength;
            }
        }

        public void AddLetterReq(int row, int col, WordDirection direction, object[] letterReq)
        {
            Undo undo = new Undo
            {
                row = row,
                col = col,
                direction = direction,
                undoType = Undo.UndoType.LetterReq,
            };
            undos[^1].Add(undo);
            cells[row][col].letterReqs[direction].Add(letterReq);
        }

        public void BeginUndo()
        {
            undos.Add(new List<Undo>());
        }

        public void UndoHandle()
        {
            List<Undo> undosToUndo = undos[^1];
            undos.RemoveAt(undos.Count - 1);
            for (int i = 0; i < undosToUndo.Count; i++)
            {
                Undo undo = undosToUndo[i];
                switch (undo.undoType)
                {
                    case Undo.UndoType.Letter:
                        cells[undo.row][undo.col].letter = (char)undo.obj;
                        break;
                    case Undo.UndoType.CanPlaceWord:
                        cells[undo.row][undo.col].canPlaceWord[undo.direction] = (bool)undo.obj;
                        break;
                    case Undo.UndoType.MaxWordLength:
                        cells[undo.row][undo.col].maxWordLength[undo.direction] = (int)undo.obj;
                        break;
                    case Undo.UndoType.LetterReq:
                        List<object[]> letterReqs = cells[undo.row][undo.col].letterReqs[undo.direction];
                        letterReqs.RemoveAt(letterReqs.Count - 1);
                        break;
                }
            }
        }
    }

    private class Cell
    {
        public CellPosition position;
        public char letter;

        public Dictionary<WordDirection, bool> canPlaceWord;
        public Dictionary<WordDirection, int> maxWordLength;
        public Dictionary<WordDirection, List<object[]>> letterReqs;
    }

    private class Undo
    {
        public enum UndoType
        {
            Letter,
            CanPlaceWord,
            MaxWordLength,
            LetterReq
        }

        public int row;
        public int col;
        public WordDirection direction;
        public UndoType undoType;
        public object obj;
    }

    #region Private Methods

    private void InitWorkingBoard(WorkingBoard workingBoard)
    {
        workingBoard.cells = new List<List<Cell>>();
        workingBoard.placedWords = new List<object[]>();
        workingBoard.undos = new List<List<Undo>>();
        
        //Init the working board cells
        for (int row = 0; row < workingBoard.boardConfig.rows; row++)
        {
            workingBoard.cells.Add(new List<Cell>());
            for (int col = 0; col < workingBoard.boardConfig.cols; col++)
            {
                Cell cell = new Cell
                {
                    position = new CellPosition(row,col),
                    letter = Board.BlankChar,
                    canPlaceWord = new Dictionary<WordDirection, bool>(),
                    maxWordLength = new Dictionary<WordDirection, int>(),
                    letterReqs = new Dictionary<WordDirection, List<object[]>>()
                };
                bool[] canPlaceWord = {
                    row > 0,																	                // Up
                    row > 0 && col < workingBoard.boardConfig.cols - 1,								            // UpRight
                    col < workingBoard.boardConfig.cols - 1,											        // Right
                    row < workingBoard.boardConfig.rows - 1 && col < workingBoard.boardConfig.cols - 1,	        // DownRight
                    row < workingBoard.boardConfig.rows - 1,											        // Down
                    row < workingBoard.boardConfig.rows - 1 && col > 0,								            // DownLeft
                    col > 0,																	                // Left
                    row > 0 && col > 0															                // UpLeft
                };

                int[] maxWordLength = {
                    row + 1,																	                // Up
                    Mathf.Min(row + 1, workingBoard.boardConfig.cols - col),							    // UpRight
                    workingBoard.boardConfig.cols - col,												        // Right
                    Mathf.Min(workingBoard.boardConfig.rows - row, workingBoard.boardConfig.cols - col),	// DownRight
                    workingBoard.boardConfig.rows - row,												        // Down
                    Mathf.Min(workingBoard.boardConfig.rows - row, col + 1),							    // DownLeft
                    col + 1, 																	                // Left
                    Mathf.Min(row + 1, col + 1), 												            // UpLeft
                };

                for (int i = 0; i < (int)WordDirection.COUNT; i++)
                {
                    WordDirection wordDirection = (WordDirection)i;

                    cell.canPlaceWord.Add(wordDirection, canPlaceWord[i]);
                    cell.maxWordLength.Add(wordDirection, maxWordLength[i]);
                    cell.letterReqs.Add(wordDirection, new List<object[]>());
                }

                workingBoard.cells[row].Add(cell);
            }
        }
    }

    private void FillBlackSpace(Board board, string randomCharacters)
    {
        for (int row = 0; row < board.rows; row++)
        {
            for (int col = 0; col < board.cols; col++)
            {
                if (board.boardCharacters[row][col] == Board.BlankChar)
                {
                    board.boardCharacters[row][col] = randomCharacters[Random.Range(0, randomCharacters.Length)];
                }
            }
        }
    }

    private bool PlaceNextWord(WorkingBoard workingBoard, int wordIndex)
    {
        if (Stopping)
        {
            return false;
        }

        if (wordIndex >= workingBoard.boardConfig.words.Count)
        {
            return true;
        }

        if (workingBoard.boardConfig.algoTimeOut > 0 &&
            timer.ElapsedMilliseconds >= workingBoard.boardConfig.algoTimeOut)
        {
            return true;
        }

        string word = workingBoard.boardConfig.words[wordIndex].Replace(" ", "");
        
        return true;
    }
    
    #endregion
}