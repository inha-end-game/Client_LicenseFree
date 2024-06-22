using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HangeulTyper : MonoBehaviour
{
    public List<TextMeshProUGUI> textMeshPro; // TextMeshPro ������Ʈ�� �Ҵ��ϼ���.
    public float minDelay = 0.1f; // Ÿ���� ���� �ð�
    public float maxDelay = 0.3f; // Ÿ���� ���� �ð�

    private List<string> originalText = new List<string>();

    void Awake()
    {
        foreach (var text in textMeshPro)
            originalText.Add(text.text);
    }

    void OnEnable()
    {
        foreach (var text in textMeshPro)
            text.text = "";

        for (int i = 0; i < originalText.Count; i++)
            StartCoroutine(TypeText(i));
    }

    IEnumerator TypeText(int idx)
    {
        string currentText = "";

        var text = originalText[idx];
        var uiText = textMeshPro[idx];

        foreach (var letter in text)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            currentText += letter;
            uiText.text = currentText;
        }
    }
}
