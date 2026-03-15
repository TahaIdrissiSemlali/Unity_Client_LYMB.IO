using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StartButtonAnimation : MonoBehaviour
{
    [SerializeField]
    private Button startButton;
    
    [SerializeField]
    public RectTransform buttonTransform;
    
    private Tween buttonTween; 

    void Start()
    {
        if (buttonTransform == null)
            buttonTransform = startButton?.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (startButton == null || buttonTransform == null) return;

        if (startButton.interactable)
        {
            if (buttonTween == null || !buttonTween.IsPlaying())
            {
                StartAnimation();
            }
        }
        else
        {
            StopAnimation();
        }
    }

    private void StartAnimation()
    {
        if (buttonTransform == null) return;

        buttonTween = buttonTransform.DOScale(new Vector3(1.2f, 1.2f, 1f), 0.6f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopAnimation()
    {
        if (buttonTween != null)
        {
            buttonTween.Kill();
            buttonTween = null;
        }

        if (buttonTransform != null) 
            buttonTransform.localScale = Vector3.one;
    }

    private void OnDestroy()
    {
        StopAnimation();
    }
}