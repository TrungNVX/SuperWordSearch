using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBG.WordSearch
{
    [Serializable]
    public class CategoryInfo
    {
        public enum LockType
        {
            None,
            Coins,
            Keys,
            IAP
        }

        public string displayName;
        public string saveId;
        public Sprite icon;
        public Color categoryColor;
        public LockType lockType;
        public int unlockAmount;
        public int iapProductId;
        public TextAsset wordFile;
        public List<TextAsset> levelFiles;
    }
}