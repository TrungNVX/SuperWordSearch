using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BBG.WordSearch
{
    [Serializable]
    public class DifficultyInfo
    {
        public int boardRowSize;
        public int boardColSize;
        public int maxWords;
        public int maxWordLength;
    }
}