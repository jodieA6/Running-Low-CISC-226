using UnityEngine;

public class SaveFileManager : MonoBehaviour
{
    private string saveFilePath;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveFilePath = Application.persistentDataPath + "/save.csv";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SaveData()
    {
        string data = STATIC_DATA.CURRENT_LEVEL + ",";
        saveFilePath = Application.persistentDataPath + "/save.csv";
        System.IO.File.WriteAllText(saveFilePath, data);
    }

    public string LoadData()
    {
        if (SaveFileExists())
        {
            string data = System.IO.File.ReadAllText(saveFilePath);
            return data;
        }
        else
        {
            Debug.LogError("Save file not found at: " + saveFilePath);
            return null;
        }
    }

    public string GetSaveLevel()
    {
        string data = LoadData();
        if (!string.IsNullOrEmpty(data))
        {
            string[] splitData = data.Split(',');
            if (splitData.Length > 0)
            {
                string savedLevel = splitData[0];
                STATIC_DATA.CURRENT_LEVEL = savedLevel;
                return savedLevel;
            }
            else
            {
                Debug.LogError("Save file is empty or corrupted.");
            }
        }
        return null;
    }

    public bool SaveFileExists()
    {
        saveFilePath = Application.persistentDataPath + "/save.csv";
        return System.IO.File.Exists(saveFilePath);
    }
}
