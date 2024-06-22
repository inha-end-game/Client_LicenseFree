using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInImage : MonoBehaviour
{
    public List<Image> images; // Image 컴포넌트를 할당합니다.
    public float duration = 2f; // 페이드 인 지속 시간

    void Start()
    {
        // 코루틴을 시작합니다.
        foreach (var image in images)
            StartCoroutine(FadeInCoroutine(image));
    }

    IEnumerator FadeInCoroutine(Image image)
    {
        float elapsedTime = 0f;
        
        Color color = image.color;

        // Image의 color 값을 0에서 1로 점진적으로 변경합니다.
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / duration);
            image.color = color;
            yield return null;
        }

        // 페이드 인이 완료되면 alpha 값을 1로 설정합니다.
        color.a = 1f;
        image.color = color;
    }
}
