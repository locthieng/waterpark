using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SingleLevelController : MonoBehaviour
{
    private LevelController level;
    [Header("Tutorial")]
    public bool IsTutorial;
    public TutHand tutHand;

    public virtual void SetUp()
    {
        level = LevelController.Instance;
    }
    public virtual void StartLevel()
    {
        
    }

    public virtual void ResetLevel()
    {
    }

    public void ShowTutHand(bool isActive)
    {
    }    

    public void CheckWin(bool isActive)
    {
    }    

    private void OnDestroy()
    {
    }

    public virtual void ShowSingleFinishEffects()
    {

    }

}
