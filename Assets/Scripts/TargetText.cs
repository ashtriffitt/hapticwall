using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TargetText : MonoBehaviour
{

    private TextMeshProUGUI targetText;

    // Start is called before the first frame update
    void Start()
    {
        targetText = gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateText(int num) {
        targetText.text = num.ToString();
    }
}
