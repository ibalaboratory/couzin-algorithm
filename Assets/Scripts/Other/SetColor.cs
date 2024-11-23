using UnityEngine;

// アタッチしたオブジェクトの色を変える．
// Playモードに入る前に設定した色はPlayモードが開始されてから反映されます．
// 注：特殊なマテリアルを使用している場合上手くいきません．
public class SetColor : MonoBehaviour
{
    public Color color;

    private Renderer selfRenderer;

    void Start()
    {
        selfRenderer = GetComponent<Renderer>();
        ChangeColor();
    }

    [ContextMenu("Change Color")]
    void ChangeColor()
    {
        if(Application.isPlaying)
        {
            if(selfRenderer?.material)
                selfRenderer.material.color = color;
        }
        else
        {
            Debug.LogWarning("ChangeColorはPlayモード中のみ使用可能です．"
            + "(Playモードに入る際に指定された色に自動的に変更されます．)\n"
            + "ChangeColor is only available in play mode. "
            + "(Color will automatically change to what you specified when entering play mode.)");
        }
    }
}
