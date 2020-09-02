using System;
using UnityEngine;

// Token: 0x02000048 RID: 72
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
public class MeshBloomPrePassLight : TubeBloomPrePassLight
{
    public Renderer renderer;

    public void Init(Renderer renderer)
    {
        this.renderer = renderer;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        _parametricBoxController.enabled = false;
        renderer.material.color = color;
    }

    internal void UpdateColor(Color color)
    {
        this.color = color;
        if (renderer.material != null)
        {
            renderer.enabled = color.a > 0;
            renderer.material.color = color;
        }
    }
}