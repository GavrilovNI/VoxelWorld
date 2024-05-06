using Sandbox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VoxelWorld.Meshing;
using VoxelWorld.Mth;
using static Sandbox.ModelRenderer;

namespace VoxelWorld.Rendering;

public class UnlimitedModelRenderer : Component, IEnumerable<ModelRenderer>
{
    protected int RequiredCount = 0;
    protected List<ModelRenderer> Renderers = new();

    public int Count => Renderers.Take(RequiredCount).Count(r => r.IsValid);

    public BBox Bounds
    {
        get
        {
            BBox? result = null;
            foreach(var renderer in this)
                result = result.AddOrCreate(renderer.Bounds);
            return result ?? new BBox();
        }
    }

    public BBox ModelBounds
    {
        get
        {
            BBox? result = null;
            foreach(var renderer in this)
            {
                if(renderer.Model is not null)
                    result = result.AddOrCreate(renderer.Model.Bounds);
            }
            return result ?? new BBox();
        }
    }

    private Material? _materialOverride = null;
    [Property, MakeDirty] public Material? MaterialOverride
    {
        set
        {
            _materialOverride = value;
            foreach(var renderer in this)
                renderer.MaterialOverride = value;
        }
        get => _materialOverride;
    }

    private Color _tint = Color.White;
    [Property, MakeDirty] public Color Tint
    {
        set
        {
            _tint = value;
            foreach(var renderer in this)
                renderer.Tint = value;
        }
        get => _tint;
    }

    private ShadowRenderType _renderType = ShadowRenderType.On;
    [Property, Title("Cast Shadows"), MakeDirty, Category("Lighting"), DefaultValue(ShadowRenderType.On)] public ShadowRenderType RenderType
    {
        set
        {
            _renderType = value;
            foreach(var renderer in this)
                renderer.RenderType = value;
        }
        get => _renderType;
    }

    public CombinedRenderAttributes SceneObjectAttrubutes { get; }

    public UnlimitedModelRenderer()
    {
        SceneObjectAttrubutes = new(this);
    }


    public void Clear() => SetupRenderers(0);

    public void SetModels(params Model[] models) => SetModels(models.AsEnumerable());

    public virtual void SetModels(IEnumerable<Model> models)
    {
        ThreadSafe.AssertIsMainThread();
        SetupRenderers(models.Count());

        int i = 0;
        foreach(var model in models)
            Renderers[i++].Model = model;
    }

    public virtual void SetModels<V>(UnlimitedMesh<V> unlimitedMesh, Material? material = null) where V : unmanaged, IVertex
    {
        ThreadSafe.AssertIsMainThread();

        var partsCount = unlimitedMesh.PartsCount;

        Model[] models = new Model[partsCount];

        for(int i = 0; i < partsCount; ++i)
        {
            ModelBuilder modelBuilder = new();
            Mesh mesh = new(material);
            unlimitedMesh.CreateBuffersFor(mesh, i);
            modelBuilder.AddMesh(mesh);
            models[i] = modelBuilder.Create();
        }

        SetModels(models);
    }


    protected virtual void SetupRenderers(int count)
    {
        var currentRenderers = Renderers.Where(r => r.IsValid).ToList();

        for(int i = currentRenderers.Count; i < count; ++i)
            currentRenderers.Add(Components.Create<ModelRenderer>());

        for(int i = currentRenderers.Count - 1; i >= count; --i)
            currentRenderers[i].Enabled = false;

        for(int i = 0; i < count; ++i)
        {
            var renderer = currentRenderers[i];
            renderer.MaterialOverride = MaterialOverride;
            renderer.Tint = Tint;
            renderer.RenderType = RenderType;
            renderer.Enabled = true;
        }

        RequiredCount = count;
        Renderers = currentRenderers;
    }

    protected override void OnEnabled()
    {
        int i = 0;
        for(; i < RequiredCount; ++i)
            Renderers[i].Enabled = true;
        for(; i < Renderers.Count; ++i)
            Renderers[i].Enabled = false;
    }

    protected override void OnDisabled()
    {
        for(int i = 0; i < Renderers.Count; ++i)
            Renderers[i].Enabled = false;
    }

    protected override void OnDestroy()
    {
        for(int i = 0; i < Renderers.Count; ++i)
            Renderers[i].Destroy();
        Renderers.Clear();
        RequiredCount = 0;
    }

    public IEnumerator<ModelRenderer> GetEnumerator()
    {
        for(int i = 0; i < RequiredCount; ++i)
        {
            var renderer = Renderers[i];
            if(renderer.IsValid)
                yield return renderer;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public class CombinedRenderAttributes : IValid
    {
        private readonly UnlimitedModelRenderer _renderer;

        public bool IsValid => _renderer.IsValid();

        public CombinedRenderAttributes(UnlimitedModelRenderer renderer)
        {
            _renderer = renderer;
        }

        protected void ForeachRendererAttributes(Action<RenderAttributes> action)
        {
            foreach(var renderer in _renderer)
                action(renderer.SceneObject.Attributes);
        }

        public void Clear() => ForeachRendererAttributes(a => a.Clear());
        public void SetCombo(string k, int value) => ForeachRendererAttributes(a => a.SetCombo(k, value));
        public void SetCombo(string k, Enum value) => ForeachRendererAttributes(a => a.SetCombo(k, value));
        public void SetCombo(string k, bool value) => ForeachRendererAttributes(a => a.SetCombo(k, value));
        public void Set(string k, int value) => ForeachRendererAttributes(a => a.Set(k, value));
        public void Set(string k, Texture value, int mip = -1) => ForeachRendererAttributes(a => a.Set(k, value, mip));
        public void Set(string k, float value) => ForeachRendererAttributes(a => a.Set(k, value));
        public void Set(string k, string value) => ForeachRendererAttributes(a => a.Set(k, value));
        public void Set(string k, bool value) => ForeachRendererAttributes(a => a.Set(k, value));
        public void Set(string k, Vector4 value) => ForeachRendererAttributes(a => a.Set(k, value));
        public void Set(string k, Angles value) => ForeachRendererAttributes(a => a.Set(k, value));
        public void Set(string k, Vector3 value) => ForeachRendererAttributes(a => a.Set(k, value));
        public void Set(string k, Vector2 value) => ForeachRendererAttributes(a => a.Set(k, value));
        public void Set<T>(string k, ComputeBuffer<T> value) where T : struct => ForeachRendererAttributes(a => a.Set(k, value));

        public void SetData<T>(string k, in Span<T> value) where T : unmanaged
        {
            foreach(var renderer in _renderer.Renderers)
                renderer.SceneObject.Attributes.SetData(k, value);
        }

        public void SetData<T>(string k, T value) where T : unmanaged =>
            ForeachRendererAttributes(a => a.SetData(k, value));

        public void SetData<T>(string k, T[] value) where T : unmanaged =>
            ForeachRendererAttributes(a => a.SetData(k, value));

        public void SetData<T>(string k, List<T> value) where T : unmanaged =>
            ForeachRendererAttributes(a => a.SetData(k, value));

        public void Set(string k, Matrix value) => ForeachRendererAttributes(a => a.Set(k, value));
    }
}
