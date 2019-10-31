using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System;


public class LogSystem : MonoBehaviour
{
    // Text file used for logging. Drag and drop file in editor.
    public TextAsset logFile;

    public Text UIText;

   // Key that is pressed to toggle the debug UI.
   public KeyCode debugToggle;

    // Start is called before the first frame update
    void Start()
    {
        // Clear the log file when scene starts.
        File.WriteAllText("logfile.txt", string.Empty);

        UIText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyUp(KeyCode.Space))
        {
            ToggleUIText();
        }
    }

    /// <summary>
    /// Toggles the UI text belonging to this LogSystem.
    /// Inputs: None
    /// Outputs: None
    /// Pre-Conditions: None
    /// Post-Conditions: If the UI was active before invoking the method it will
    /// be inactive after. If it was inactive, it will be active.
    /// </summary>
    public void ToggleUIText()
    {
        if (UIText.gameObject.activeSelf)
        {
            UIText.gameObject.SetActive(false);
        }
        else
        {
            UIText.gameObject.SetActive(true);
            PrintToTextField();
        }
    }


    public void WriteToFile(string text)
    {

        if (logFile != null)
        {
            // Build stream writer for the log file.
            StreamWriter sw = new StreamWriter("logfile.txt", append: true);
            // Prepend time to text.
            string finalAnswer = DateTime.Now.ToString("h:mm:ss tt") + ": " + text;

            sw.WriteLine(finalAnswer);
            sw.Close();

        }
    }

    public void PrintToTextField()
    {
        if (logFile != null)
        {
            StreamReader sr = new StreamReader("logfile.txt");
            UIText.GetComponent<Text>().text = sr.ReadToEnd();
            sr.Close();
        }

    }
}
