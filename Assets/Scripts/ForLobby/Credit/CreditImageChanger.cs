using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditImageChanger : MonoBehaviour
{
    public Image displayImage; // �̹����� ǥ���� UI Image ������Ʈ
    public Sprite[] images; // ��ȯ�� �̹������� �迭
    public float fadeDuration = 1f; // ���̵� �ƿ�/�ο� �ɸ��� �ð�
    public float displayDuration = 3f; // �� �̹����� ǥ���� �ð� (���̵� �ð� ����)

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

            // ���̵� �ƿ�
            yield return StartCoroutine(FadeOut());

            // ���� �̹����� ����
            currentImageIndex = (currentImageIndex + 1);

            if (currentImageIndex == images.Length)
                break;

            displayImage.sprite = images[currentImageIndex];

            // ���̵� ��
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

        // ���� �����ϰ� ����
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

        // ���� �������ϰ� ����
        Color finalColor = displayImage.color;
        finalColor.a = 1;
        displayImage.color = finalColor;
    }
}
