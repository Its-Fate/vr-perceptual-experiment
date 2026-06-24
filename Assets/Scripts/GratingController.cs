using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class GratingController : MonoBehaviour
{
    [Header("Grating Parameters")]
    public float speed = 1.0f;
    public float frequency = 10f;
    public float contrast = 1.0f;
    public Vector2 direction = new Vector2(1, 0);

    private MaterialPropertyBlock mpb;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
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
            rend.SetPropertyBlock(mpb);
        }
        else
        {
            return;
        }
    }
}