using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCreatorBehaviour : MonoBehaviour
{
    private void Update()
    {
        if (BoardCreator.IsFinished)
        {
            BoardCreator.OnBoardWorkerFinished();
        }
    }
}
