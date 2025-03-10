using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class AutoGenerateButtons : MonoBehaviour
{
    public GameObject buttonPrefab; // Префаб кнопки
    public Transform parentPanel; // Родительский объект для кнопок
    public List<string> urls; // Список ссылок

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

            TextMeshPro buttonText = newButton.GetComponentInChildren<TextMeshPro>();
            if (buttonText != null)
            {
                buttonText.text = "Ссылка " + (i + 1);
            }

            Button buttonComponent = newButton.GetComponent<Button>();
            if (buttonComponent != null)
            {
                string url = urls[i]; // Локальная копия для предотвращения замыкания
                buttonComponent.onClick.AddListener(() => OpenLink(url));
            }
        }
    }

    void OpenLink(string url)
    {
        Application.OpenURL(url);
    }
}
