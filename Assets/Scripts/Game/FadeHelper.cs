using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public static class FadeHelper
{
    public static void FadeIn(Image image, float duration, Action onComplete = null)
    {
        image.gameObject.SetActive(true);
        image.color = new Color(0, 0, 0, 0);
        var tween = image.DOFade(1, duration);
        if (onComplete != null)
            tween.OnComplete(() => onComplete());
    }

    public static void FadeOut(Image image, float duration, Action onComplete = null)
    {
        image.color = new Color(0, 0, 0, 1);
        var tween = image.DOFade(0, duration);
        tween.OnComplete(() =>
        {
            if (image != null)
                image.gameObject.SetActive(false);
            if (onComplete != null)
                onComplete();
        });
    }
}
