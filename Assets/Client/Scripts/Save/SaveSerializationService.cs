using System;
using System.IO;
using UnityEngine;

public class SaveSerializationService
{
	private readonly object fileLock = new object();

	private string GetDirectory()
	{
		//string directory = Path.Combine(folderPath);
		string directory = Path.Combine(Application.persistentDataPath, "Saves");
		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}

		return directory;
	}

	private string GetFilePath(string fileName)
	{
		return Path.Combine(GetDirectory(), fileName + ".json");
	}

	public void SaveAsync<T>(string fileName, T saveData)
	{
		try
		{
			string json = JsonUtility.ToJson(saveData);
			string path = GetFilePath(fileName);

			lock (fileLock)
			{
				File.WriteAllText(path, json);
			}
		}
		catch (Exception e)
		{
			Debug.LogError($"Error saving file {fileName}: {e.Message}");
		}
	}

	public T Load<T>(string fileName) where T : class, new()
	{
		try
		{
			string path = GetFilePath(fileName);
			if (!File.Exists(path))
			{
				return new T();
			}

			string json = File.ReadAllText(path);
			return JsonUtility.FromJson<T>(json);
		}
		catch (Exception e)
		{
			Debug.LogError($"Error loading file {fileName}: {e.Message}");
			return new T();
		}
	}

	public bool FileExists(string fileName)
	{
		return File.Exists(GetFilePath(fileName));
	}

	public void DeleteFile(string fileName)
	{
		string path = GetFilePath(fileName);
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}
}
