using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(BoxCollider))]
public class ButtonBehavior : MonoBehaviour
{
    #region Transparency
    [Header("Transparency")]
    [SerializeField] private float idleAlpha;
    [SerializeField] private float highlightedAlpha;
    [SerializeField] private float pressedAlpha;
    #endregion

    private Material material;

    private void Start()
    {
        material = GetComponent<Renderer>().material;
        Idle();
    }

    public void Highlighted()
    {
        Color c = material.color;
        c.a = highlightedAlpha;
        material.color = c;
    }

    public void Pressed()
    {
        Color c = material.color;
        c.a = pressedAlpha;
        material.color = c;
    }

    public void Idle()
    {
        Color c = material.color;
        c.a = idleAlpha;
        material.color = c;
    }
}
