using UnityEngine;
using TMPro;
public class timeDisplay : MonoBehaviour
{

    Master myMaster;
    TextMeshProUGUI myTextMeshPro;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myMaster = GetComponentInParent<Master>();
        myTextMeshPro = GetComponent<TextMeshProUGUI>();    
    }

    // Update is called once per frame
    void Update()
    {
        myTextMeshPro.text = "Time Dead: " + Mathf.FloorToInt(myMaster.timeFromStart).ToString();
    }
}
    