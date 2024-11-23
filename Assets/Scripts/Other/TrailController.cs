using UnityEngine;

// TrailRendererの設定を変更する．
public class TrailController : MonoBehaviour
{
    private GameObject trailObject;
    public TrailRenderer trailRenderer;

    // 軌跡の持続時間[s]
    public float duration;
    // 軌跡の色
    public Color color;

    void Start()
    {
        trailObject = transform.Find("Trail").gameObject;
        if(trailObject)
        {
            trailRenderer = trailObject.GetComponent<TrailRenderer>();
            duration = trailRenderer.time;
            color = trailRenderer.startColor;
        }
    }

    public void EnableTrail()
    {
        if(trailObject) trailObject.SetActive(true);
    }

    public void DisableTrail()
    {
        if(trailObject) trailObject.SetActive(false);
    }

    public void RandomizeColor()
    {
        if(trailRenderer) 
        {
            color = new Color(Random.value, Random.value, Random.value);
            trailRenderer.startColor = color;
        }
    }
}
