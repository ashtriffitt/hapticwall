using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderControl : MonoBehaviour
{

    private TrialManagerScaling trialManager;
    [SerializeField]
    private ViveController controller;
    private Slider slider;

    // Start is called before the first frame update
    void Start()
    {
        trialManager = GameObject.Find("Trial Manager").GetComponent<TrialManagerScaling>();
        slider = gameObject.GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.leftClick && trialManager.canMoveSlider) {
            slider.value--;
        }
        if (controller.rightClick && trialManager.canMoveSlider) {
            slider.value++;
        }
    }

    public void Holding(int dir)
    {
        slider.value += dir;
    }
}
