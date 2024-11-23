using UnityEngine;

public class TimeScaleController : MonoBehaviour {
    [Range(-5f, 20f), Tooltip("Time scale in dB.")]
    public float timeScale = 0f;

    void Update() {
        Time.timeScale = Mathf.Pow(10f, timeScale / 10f);
    }

    public void OnTimeScaleSliderChange(float timeScale) {
        this.timeScale = timeScale;
    }
}