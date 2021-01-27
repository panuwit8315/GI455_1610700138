using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextMatching : MonoBehaviour
{
    public InputField inputFieldArea;

    string inputText;
    public Text resultText;

    string[] flowerName = {"Sunflower", "Lotus", "Rose", "Tulip", "Daisy", "Iris", "Jasmine", "Lily", "Orchid" };

    public void OnClickFindBtn()
    {       
        inputText = inputFieldArea.text;
        string result = "[";        
        Debug.Log("Input ---> " + inputText);        

        for(int i = 0; i < flowerName.Length; i++)
        {
            if(inputText == flowerName[i])
            {
                Debug.Log("Found ----> " + inputText);
                result += $" <color=green> {inputText} </color>";
                result += "] is found.";
                resultText.text = result;
                return;
            }
        }
        Debug.Log("Not Found!!");
        result += $" <color=red> {inputText} </color>";
        result += "] is not found.";
        resultText.text = result;
    }
    
}
