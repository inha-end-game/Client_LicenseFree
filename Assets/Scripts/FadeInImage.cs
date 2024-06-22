using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInImage : MonoBehaviour
{
    public List<Image> images; // Image ������Ʈ�� �Ҵ��մϴ�.
    public float duration = 2f; // ���̵� �� ���� �ð�

    void Start()
    {
        // �ڷ�ƾ�� �����մϴ�.
        foreach (var image in images)
            StartCoroutine(FadeInCoroutine(image));
    }

    IEnumerator FadeInCoroutine(Image image)
    {
        float elapsedTime = 0f;
        
        Color color = image.color;

        // Image�� color ���� 0���� 1�� ���������� �����մϴ�.
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / duration);
            image.color = color;
            yield return null;
        }

        // ���̵� ���� �Ϸ�Ǹ� alpha ���� 1�� �����մϴ�.
        color.a = 1f;
        image.color = color;
    }
}
