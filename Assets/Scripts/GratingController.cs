using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class GratingController : MonoBehaviour
{
    [Header("Grating Parameters")]
    public float speed = 5.0f;
    public float frequency = 1.15f;
    public float contrast = 1.0f;
    public Vector2 direction = new Vector2(1, 0);
    public float gaussianSharpness = 82f;

    private MaterialPropertyBlock mpb;
    private Renderer rend;

    void Start()
    {
        try {rend = GetComponent<Renderer>();}
        catch (System.Exception e) {Debug.LogError("GratingController: Renderer component not found. " + e.Message);}
        mpb = new MaterialPropertyBlock();
        UpdateGrating();
    }

    void Update()
    {
        UpdateGrating();
    }

    void UpdateGrating()
    {
        if (rend != null && mpb != null)
        {
            rend.GetPropertyBlock(mpb);
            mpb.SetFloat("_Speed", speed);
            mpb.SetFloat("_Frequency", frequency);
            mpb.SetFloat("_Contrast", contrast);
            mpb.SetVector("_Direction", direction);
            mpb.SetFloat("_GaussianSharpness", gaussianSharpness);
            rend.SetPropertyBlock(mpb);
        }
    }
}