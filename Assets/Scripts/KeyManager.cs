using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyManager : MonoBehaviour
{
    [SerializeField] private Slider mouseSensitiveSlider;
    private string key = "MouseSensitive";

    private void Start()
    {
        mouseSensitiveSlider.value = PlayerPrefs.GetFloat(key, 0.5f);
        SetMouseSensitive();
    }

    public void ChangeValue(float value)
    {
        SetMouseSensitive(value);
    }

    private void SetMouseSensitive(float value = 0.5f)
    {
        PlayerController.mouseSensitive = value;
        PlayerPrefs.SetFloat(key, value);
    }
}
