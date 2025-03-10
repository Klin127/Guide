using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class ButtonSpawn : MonoBehaviour
{
    public string repoOwner = "Klin127"; // Логин владельца репозитория
    public string repoName = "SchoolProject"; // Название репозитория
    public string folderPath = "Themes"; // Путь к папке в репозитории (или "" если корневая)
    public GameObject textPanel;
    public GameObject buttonsPanel;
    
    public Transform buttonContainer; // Контейнер для кнопок
    public Button buttonPrefab; // Префаб кнопки
    public TextMeshProUGUI textMeshPro; // Общий текстовый элемент

    private string apiUrl => $"https://api.github.com/repos/{repoOwner}/{repoName}/contents/{folderPath}";

    void Start()
    {
        StartCoroutine(GetFilesFromGitHub());
    }

    IEnumerator GetFilesFromGitHub()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("User-Agent", "Unity");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Ошибка загрузки: " + request.error);
            yield break;
        }

        GitHubFile[] files = JsonHelper.FromJson<GitHubFile>(request.downloadHandler.text);
        foreach (GitHubFile file in files)
        {
            if (file.name.EndsWith(".txt")) // Проверяем, чтобы загружать только .txt файлы
                CreateButton(file.name, file.download_url);
        }
    }

    void CreateButton(string fileName, string fileUrl)
    {
        Button newButton = Instantiate(buttonPrefab, buttonContainer);
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = fileName;
        newButton.onClick.AddListener(() => {
       StartCoroutine(LoadFileContent(fileUrl));
       textPanel.SetActive(true);
       buttonsPanel.SetActive(false);
   });
    }

    IEnumerator LoadFileContent(string fileUrl)
    {
        UnityWebRequest request = UnityWebRequest.Get(fileUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Ошибка загрузки файла: " + request.error);
            yield break;
        }

        textMeshPro.text = request.downloadHandler.text; // Обновляем общий текстовый блок
    }
}

// Вспомогательный класс для JsonUtility
[System.Serializable]
public class GitHubFile
{
    public string name;
    public string download_url;
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{ \"files\": " + json + "}";
        return JsonUtility.FromJson<Wrapper<T>>(newJson).files;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] files;
    }
}