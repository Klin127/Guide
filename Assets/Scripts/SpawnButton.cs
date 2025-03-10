using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class SpawnButton : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform contentPanel;
    [SerializeField] private string githubUsername = "Klin127";
    [SerializeField] private string repositoryName = "SchoolProject";
    [SerializeField] private string branch = "main";
    [SerializeField] private string folderPath = "Presentations";

    private string baseUrl = "https://github.com/";
    private List<string> fileNames = new List<string>();

    [System.Serializable]
    private class GitHubFile
    {
        public string name;
        public string path;
        public string type;
    }

    [System.Serializable]
    private class GitHubFileList
    {
        public GitHubFile[] Items;
    }

    void Start()
    {
        StartCoroutine(GetFilesFromGitHub());
    }

    IEnumerator GetFilesFromGitHub()
    {
        string apiUrl = $"https://api.github.com/repos/{githubUsername}/{repositoryName}/contents/{folderPath}?ref={branch}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            request.SetRequestHeader("User-Agent", "Unity-GitHub-Request");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                ProcessGitHubResponse(jsonResponse);
                CreateButtons();
            }
            else
            {
                Debug.LogError($"Error fetching GitHub files: {request.error}");
            }
        }
    }

    void ProcessGitHubResponse(string json)
    {
        string wrappedJson = "{\"Items\":" + json + "}";
        GitHubFileList fileList = JsonUtility.FromJson<GitHubFileList>(wrappedJson);
        
        fileNames.Clear();
        foreach (var file in fileList.Items)
        {
            if (file.type == "file")
            {
                fileNames.Add(file.name);
                Debug.Log($"Found file: {file.name}");
            }
        }
        Debug.Log($"Total files: {fileNames.Count}");
    }

   void CreateButtons()
{
    foreach (string fileName in fileNames)
    {
        GameObject buttonObj = Instantiate(buttonPrefab, contentPanel);
        Button button = buttonObj.GetComponent<Button>();
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

        if (buttonText != null)
        {
            Debug.Log($"Setting button text to: {fileName}");
            buttonText.text = fileName;
        }
        else
        {
            Debug.LogError("Text component not found in prefab!");
        }

        string fileUrl = $"{baseUrl}{githubUsername}/{repositoryName}/blob/{branch}/{folderPath}/{fileName}";
        button.onClick.AddListener(() => 
        {
            Debug.Log($"Button clicked for file: {fileName}, URL: {fileUrl}");
            OpenLink(fileUrl);
        });
    }
}

    // Метод для кроссплатформенного открытия URL
private void OpenLink(string url)
{
    Debug.Log($"Attempting to open URL: {url}");

#if UNITY_ANDROID && !UNITY_EDITOR
    try
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject uri = new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>("parse", url);
        AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW", uri);
        // Добавляем флаг для явного вызова активности
        intent.Call<AndroidJavaObject>("addFlags", 0x10000000); // FLAG_ACTIVITY_NEW_TASK
        currentActivity.Call("startActivity", intent);
        Debug.Log("URL opened via Android Intent with FLAG_ACTIVITY_NEW_TASK");
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Failed to open URL via Intent: {e.Message}");
        Application.OpenURL(url);
        Debug.Log("Fallback to Application.OpenURL executed");
    }
#else
    Application.OpenURL(url);
    Debug.Log("Application.OpenURL executed");
#endif
}
}