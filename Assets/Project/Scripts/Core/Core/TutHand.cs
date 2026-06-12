using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum TutorialType
{
    Move,
    Tap
}
[Serializable]
public class Stage
{
    public int stageIndex;
    public int stepIndex;
}

[Serializable]
public class TutStage
{
    public List<TutStep> tutSteps = new List<TutStep>();
    public bool IsDone { get; set; }
    public int TutStepIndex;

    public bool CheckDone()
    {
        for (int i = 0; i < tutSteps.Count; i++)
        {
            if (!tutSteps[i].IsDone)
            {
                return false;
            }
        }
        return true;
    }
}
[Serializable]
public class TutStep
{
    public bool IsDone { get; set; }
    public TutorialType Type;
    public Transform[] Nodes;
}

public class TutHand : MonoBehaviour
{
    [SerializeField] private SpriteRenderer hand;
    [SerializeField] private Animator animator;
    public List<TutStage> TutStages = new List<TutStage>();
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private float handMoveSpeed = 10f;
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool moveLocal;
    public int TutStageIndex;
    private Coroutine currentCoroutine;

    private void OnEnable()
    {
        if (autoStart)
        {
            Show();
        }
    }

    public void Show()
    {
        StopAndHide();
        TutStage currentTutStage = TutStages[TutStageIndex];
        int currentTutStepIndex = currentTutStage.TutStepIndex;
        if (TutStageIndex < TutStages.Count)
        {
            if (TutStages[TutStageIndex].IsDone)
            {
                TutStageIndex++;
                Show();
                return;
            }
            if (currentTutStage.tutSteps[currentTutStepIndex].IsDone)
            {
                TutStages[TutStageIndex].TutStepIndex++;
                Show();
                return;
            }
            switch (currentTutStage.tutSteps[currentTutStepIndex].Type)
            {
                case TutorialType.Move:
                    if (animator != null)
                    {
                        animator.enabled = false;
                    }
                    Vector3[] path = new Vector3[currentTutStage.tutSteps[currentTutStepIndex].Nodes.Length];
                    for (int i = 0; i < path.Length; i++)
                    {
                        if (moveLocal)
                        {
                            path[i] = currentTutStage.tutSteps[currentTutStepIndex].Nodes[i].localPosition;
                        }
                        else
                        {
                            path[i] = currentTutStage.tutSteps[currentTutStepIndex].Nodes[i].position;
                        }
                    }
                    StartMoving(path);
                    break;
                case TutorialType.Tap:
                    hand.color = new Color(1, 1, 1, 1);
                    hand.gameObject.SetActive(true);
                    if (currentTutStage.tutSteps[currentTutStepIndex].Nodes != null && currentTutStage.tutSteps[currentTutStepIndex].Nodes.Length > 0)
                    {
                        transform.position = currentTutStage.tutSteps[currentTutStepIndex].Nodes[0].position;
                    }
                    hand.enabled = true;
                    if (animator != null)
                    {
                        animator.enabled = true;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public void StartMoving(Vector3[] points)
    {
        StopAndHide();

        currentCoroutine = StartCoroutine(CoStartTut(points));
    }

    IEnumerator CoStartTut(Vector3[] points)
    {
        if (!gameObject.activeSelf) yield break;
        yield return new WaitForSeconds(0.5f);
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = false;
        }
        yield return new WaitForSeconds(0.1f);
        if (moveLocal)
        {
            transform.localPosition = points[0];
        }
        else
        {
            transform.position = points[0];
        }

        LeanTween.alpha(hand.gameObject, 1, 0.2f);
        hand.transform.localScale = Vector3.one;
        yield return new WaitForSeconds(0.1f);
        hand.gameObject.SetActive(true);
        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
        }
        if (moveLocal)
        {
            gameObject.LeanMoveLocalThroughPoints(points, 1, handMoveSpeed, 0.0f, false, () =>
            {
                LeanTween.alpha(hand.gameObject, 0, 0.3f).setOnComplete(() =>
                {
                    currentCoroutine = StartCoroutine(CoStartTut(points));
                });
            });
        }
        else
        {
            gameObject.LeanMoveThroughPoints(points, 1, handMoveSpeed, 0.0f, false, () =>
            {
                LeanTween.alpha(hand.gameObject, 0, 0.3f).setOnComplete(() =>
                {
                    if(gameObject.activeSelf)
                    currentCoroutine = StartCoroutine(CoStartTut(points));
                });
            });
        }

    }

    public void StopAndHide()
    {
        gameObject.LeanCancel();
        LeanTween.cancel(hand.gameObject);
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = false;
        }
        hand.gameObject.SetActive(false);
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
    }

    int stageDone;
    internal void FinishTutStep(int stage, int step)
    {
        Debug.Log("Finish");
        TutStages[stage].tutSteps[step].IsDone = true;
        if (TutStages[stage].CheckDone())
        {
            TutStages[stage].IsDone = true;
            stageDone++;
        }
        if (gameObject.activeInHierarchy)
        {
            if (TutStageIndex == stage && TutStages[stage].TutStepIndex == step)
            {
                TutStages[stage].TutStepIndex++;
                if (TutStages[stage].IsDone)
                {
                    StopAndHide();
                    gameObject.SetActive(false);
                    TutStageIndex++;
                    //if (stageDone < TutStages.Count)
                        //GameUIController.Instance.SetButtonHintActive(true);
                }
                else
                {
                    Show();
                }
            }
        }
    }

}
