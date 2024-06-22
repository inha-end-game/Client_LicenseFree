using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class JsonReader : MonoBehaviour
{
    private static Dictionary<string, Dictionary<string, object>> jsons = new Dictionary<string, Dictionary<string, object>>();

    void Awake()
    {
        //model레포와 연결할경우 상단, 빌드에 포함시킬경우 하단
        //string dataFolderPath = Path.Combine(Application.dataPath,"../..", "model");
        string dataFolderPath = Path.Combine(Application.streamingAssetsPath, "model");
        ReadJsonFilesInFolder(dataFolderPath);
        Debug.Log("success");
    }

    public static object GetModel(string fileName, string key)
    {
        if (jsons.ContainsKey(fileName))
        {
            var fileData = jsons[fileName];
            if (fileData.ContainsKey(key))
            {
                return fileData[key];
            }
        }
        return null;
    }

    private static void ReadJsonFilesInFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            throw new ArgumentException("The provided path is not a directory.");

        string[] files = Directory.GetFiles(folderPath, "*.json");
        foreach (string filePath in files)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                var rows = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonContent);
                var dataMap = new Dictionary<string, object>();
                foreach (var row in rows)
                {
                    var id = row["id"].ToString();
                    dataMap[id] = row;
                }

                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                jsons[fileNameWithoutExtension] = dataMap;
            }
            catch (Exception e)
            {
                Debug.LogError("Error reading JSON file: " + Path.GetFileName(filePath));
                Debug.LogError(e.ToString());
            }
        }
    }
}