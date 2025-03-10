using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class AutoGenerateButtonsWithRetry : MonoBehaviour
{
    public GameObject buttonPrefab; // Префаб кнопки
    public Transform parentPanel; // Родительский объект для кнопок
    public List<string> urls; // Список ссылок
    private const int MaxRetries = 3; // Максимальное количество попыток загрузки названия

    void Start()
    {
        GenerateButtons();
    }

    void GenerateButtons()
    {
        for (int i = 0; i < urls.Count; i++)
        {
            GameObject newButton = Instantiate(buttonPrefab, parentPanel);
            newButton.name = "Button_" + (i + 1);

            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Загрузка..."; // Временно ставим текст "Загрузка..."
            }

            Button buttonComponent = newButton.GetComponent<Button>();
            if (buttonComponent != null)
            {
                string url = urls[i]; // Локальная копия для предотвращения замыкания
                buttonComponent.onClick.AddListener(() => OpenLink(url));
                StartCoroutine(SetButtonTitle(newButton, url, 0)); // Первая попытка загрузки
            }
        }
    }

    IEnumerator SetButtonTitle(GameObject button, string url, int attempt)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string title = ExtractTitle(request.downloadHandler.text);
                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

                if (buttonText != null && !string.IsNullOrEmpty(title))
                {
                    buttonText.text = title;
                }
                else
                {
                    yield return RetryLoading(button, url, attempt);
                }
            }
            else
            {
                Debug.LogError($"Ошибка загрузки страницы ({attempt + 1} попытка): {request.error}");
                yield return RetryLoading(button, url, attempt);
            }
        }
    }

    IEnumerator RetryLoading(GameObject button, string url, int attempt)
    {
        if (attempt < MaxRetries)
        {
            yield return new WaitForSeconds(5f); // Ждём 5 секунд перед повторной попыткой
            StartCoroutine(SetButtonTitle(button, url, attempt + 1));
        }
        else
        {
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Видео (нет названия)";
            }
        }
    }

    string ExtractTitle(string html)
    {
        int titleStart = html.IndexOf("<title>");
        int titleEnd = html.IndexOf("</title>");
        if (titleStart != -1 && titleEnd != -1)
        {
            return html.Substring(titleStart + 7, titleEnd - titleStart - 7).Trim();
        }
        return null;
    }

    void OpenLink(string url)
    {
        Application.OpenURL(url);
    }
}