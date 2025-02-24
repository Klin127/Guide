using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;

public class ButtonSpawn : MonoBehaviour
{
    public string repoOwner = "Klin127"; // Укажи владельца репозитория
    public string repoName = "SchoolProject"; // Укажи название репозитория
    public string folderPath = "Themes"; // Укажи путь к папке в репозитории
    public Transform buttonContainer; // Контейнер для кнопок
    public Button buttonPrefab; // Префаб кнопки

    private string apiUrl => $"https://api.github.com/repos/{repoOwner}/{repoName}/contents/{folderPath}";

    void Start()
    {
        StartCoroutine(GetFilesFromGitHub());
    }

    IEnumerator GetFilesFromGitHub()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("User-Agent", "Unity"); // GitHub требует User-Agent
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Ошибка загрузки: " + request.error);
            yield break;
        }

        JArray files = JArray.Parse(request.downloadHandler.text);
        foreach (JObject file in files)
        {
            string fileName = file["name"].ToString();
            CreateButton(fileName);
        }
    }

    void CreateButton(string fileName)
    {
        Button newButton = Instantiate(buttonPrefab, buttonContainer);
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = fileName;
        newButton.onClick.AddListener(() => Debug.Log("Нажата кнопка: " + fileName));
    }
}