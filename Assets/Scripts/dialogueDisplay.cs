using UnityEngine;
using TMPro;

public class dialogueDisplay : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void showMessage(string message)
    {
        TextMeshProUGUI myText = GetComponent<TextMeshProUGUI>();
        myText.text = message;
    }
}
