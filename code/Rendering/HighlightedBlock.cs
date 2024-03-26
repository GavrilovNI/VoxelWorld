using Sandbox;
using VoxelWorld.Blocks.States;
using VoxelWorld.Meshing;

namespace VoxelWorld.Rendering;

public class HighlightedBlock : Component
{
    [Property, RequireComponent] protected UnlimitedModelRenderer ModelRenderer { get; set; } = null!;
    [Property] protected Material Material { get; set; } = null!;

    private BlockState? _blockState;
    public BlockState BlockState
    {
        get => _blockState ?? BlockState.Air;
        set
        {
            if (BlockState == value)
                return;

            _blockState = value;
            if(Active)
                UpdateRenderer();
        }
    }


    public virtual void UpdateRenderer()
    {
        if(BlockState.IsAir())
        {
            ModelRenderer.Enabled = false;
            return;
        }

        var modelMeshBuilder = new UnlimitedMesh<Vector3Vertex>.Builder()
            .Add(GameController.Instance!.BlockMeshes.GetInteraction(BlockState)!);
        //modelMeshBuilder = modelMeshBuilder.Scale(modelMeshBuilder.Bounds.Center, 1f + 1f / modelMeshBuilder.Bounds.Size);

        var modelMesh = modelMeshBuilder.Build();

        ModelRenderer.SetModels(modelMesh, Material);
        ModelRenderer.Enabled = Active;
    }

    protected override void OnEnabled()
    {
        UpdateRenderer();
        ModelRenderer.Enabled = true;
    }

    protected override void OnDisabled() => ModelRenderer.Enabled = false;
}
