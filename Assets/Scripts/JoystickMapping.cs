using UnityEngine;
using System.IO;

[System.Serializable]
public class JoystickMapping
{
    public int BlueA = 1;
    public int BlueB = 2;
    public int BlueC = 3;
    public int BlueD = 4;
    public int Admin = 5;
    public int GreenA = 6;
    public int GreenB = 7;
    public int GreenC = 8;
    public int GreenD = 9;
    public bool TestMode = false;

    public float PVPStartHealth = 12000;
    public float MusicVolume = 0.3f;
    public int RoundCount = 5;
    public bool FullSpread = true;
    public float LoadingScreenSeconds = 5;

    public static JoystickMapping CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<JoystickMapping>(jsonString);
    }

    public static JoystickMapping LoadFromFile(string sFileName)
    {
        string sCurrentDirectory = Directory.GetCurrentDirectory();
        Debug.Log(sCurrentDirectory);

        string sFullPath = Path.Combine(sCurrentDirectory, sFileName);
        if (File.Exists(sFullPath))
        {
            return CreateFromJSON(File.ReadAllText(sFullPath));
        }
        else
        {
            return new JoystickMapping();
        }
    }

    ///{
    ///"BlueA":"1",

    // Given JSON input:
    // {"name":"Dr Charles","lives":3,"health":0.8}
    // this example will return a PlayerInfo object with
    // name == "Dr Charles", lives == 3, and health == 0.8f.

}