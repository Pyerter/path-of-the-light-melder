using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteShaderController))]
public class FlashShaderController : MonoBehaviour
{
    [SerializeField] protected SpriteShaderController shaderController;
    public SpriteShaderController ShaderController { get { return shaderController; } }

    [SerializeField] protected string flashShaderName = "_flash_shader";
    public string FlashShaderName { get {  return flashShaderName; } }

    [SerializeField] protected Material flashShader;
    [SerializeField] protected Color flashColor;
    [SerializeField] protected float flashFadeSpeed = 0.02f;

    private void Awake()
    {
        if (shaderController == null)
            shaderController = GetComponent<SpriteShaderController>();

    }

    private void Start()
    {
        if (flashShader != null)
        {
            flashShader = Instantiate(flashShader);
            shaderController[flashShaderName] = flashShader;
        }
        else
        {
            Debug.LogWarning("Flash Shader Controller trying to set the flash shader material but no material is provided.");
        }
    }

    public float FlashValue { get { return flashShader.GetFloat("_ColorScale"); } set { flashShader.SetFloat("_ColorScale", Mathf.Clamp(value, 0, 1)); } }

    public void Flash()
    {
        FlashValue = 1;
    }

    private void FixedUpdate()
    {
        float flash = FlashValue;
        if (flash > 0)
        {
            FlashValue = Mathf.MoveTowards(flash, 0, flashFadeSpeed);
        }
    }
}
