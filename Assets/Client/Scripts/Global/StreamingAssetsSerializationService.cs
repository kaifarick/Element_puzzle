using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Threading.Tasks;
using System;

public class StreamingAssetsSerializationService
{
    public async Task<string> LoadFileAsync(string filePath)
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, filePath);
        
        if (NeedWebRequest(fullPath))
        {
            return await LoadViaWebRequest(fullPath);
        }
        else
        {
            return await LoadViaFileSystem(fullPath);
        }
    }


    private bool NeedWebRequest(string path)
    {
        return path.Contains("://") || path.Contains(":///");
    }

    private async Task<string> LoadViaWebRequest(string path)
    {
        UnityWebRequest request = UnityWebRequest.Get(path);
        var operation = request.SendWebRequest();
            
        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log($"File not found: {path}");
            return null;
        }

        return request.downloadHandler.text;
    }

    private static async Task<string> LoadViaFileSystem(string path)
    {
        try
        {
            return await Task.Run(() => 
            {
                if (!File.Exists(path))
                {
                    Debug.Log($"File not found: {path}");
                    return null;
                }

                return File.ReadAllText(path); 
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"File read error: {ex.Message}");
            throw;
        }
    }
}