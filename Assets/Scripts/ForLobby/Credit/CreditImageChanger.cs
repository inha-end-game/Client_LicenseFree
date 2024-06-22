using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditImageChanger : MonoBehaviour
{
    public Image displayImage; // 이미지를 표시할 UI Image 컴포넌트
    public Sprite[] images; // 전환할 이미지들의 배열
    public float fadeDuration = 1f; // 페이드 아웃/인에 걸리는 시간
    public float displayDuration = 3f; // 각 이미지를 표시할 시간 (페이드 시간 제외)

    private int currentImageIndex = 0;
    private float fadeSpeed;

    void OnEnable()
    {
        currentImageIndex = 0;

        if (images.Length > 0)
        {
            displayImage.sprite = images[currentImageIndex];
            fadeSpeed = 1f / fadeDuration;
            StartCoroutine(FadeImages());
        }
        else
        {
            Debug.LogError("No images assigned in the ImageFader script.");
        }
    }

    IEnumerator FadeImages()
    {
        while (true)
        {
            yield return new WaitForSeconds(displayDuration);

            // 페이드 아웃
            yield return StartCoroutine(FadeOut());

            // 다음 이미지로 변경
            currentImageIndex = (currentImageIndex + 1);

            if (currentImageIndex == images.Length)
                break;

            displayImage.sprite = images[currentImageIndex];

            // 페이드 인
            yield return StartCoroutine(FadeIn());
        }
    }

    IEnumerator FadeOut()
    {
        for (float t = 0; t < 1; t += Time.deltaTime * fadeSpeed)
        {
            Color color = displayImage.color;
            color.a = 1 - t;
            displayImage.color = color;
            yield return null;
        }

        // 완전 투명하게 설정
        Color finalColor = displayImage.color;
        finalColor.a = 0;
        displayImage.color = finalColor;
    }

    IEnumerator FadeIn()
    {
        for (float t = 0; t < 1; t += Time.deltaTime * fadeSpeed)
        {
            Color color = displayImage.color;
            color.a = t;
            displayImage.color = color;
            yield return null;
        }

        // 완전 불투명하게 설정
        Color finalColor = displayImage.color;
        finalColor.a = 1;
        displayImage.color = finalColor;
    }
}
