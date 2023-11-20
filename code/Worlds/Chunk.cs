using Sandbox;
using Sandcube.Mth;
using Sandcube.Worlds.Blocks.States;
using Sandcube.Worlds.Generation;
using System.Collections.Generic;

namespace Sandcube.Worlds;

public class Chunk : BaseComponent, IBlockStateAccessor
{
    [Property] public Vector3Int Position { get; init; }
    [Property] public Vector3Int Size { get; init; } = 16;
    [Property] public Vector3 VoxelSize { get; init; } = Vector3.One * 39.37f;

    [Property] public Material VoxelsMaterial { get; set; } = null!;

    protected bool _meshRebuildRequired = false;

    protected readonly List<ModelComponent> _modelComponents = new();
    protected ModelCollider _modelCollider = null!;

    protected readonly Dictionary<Vector3Int, BlockState> _blockStates = new();

    public Vector3Int BlockOffset => Position * Size;

    public Chunk()
    {
    }

    public Chunk(Vector3Int position, Vector3Int size, Vector3 voxelSize)
    {
        Position = position;
        Size = size;
        VoxelSize = voxelSize;
    }

    public Chunk(Vector3Int position, Vector3Int size) : this(position, size, Vector3.One * 39.37f)
    {
    }

    public BlockState GetBlockState(Vector3Int position) => _blockStates.GetValueOrDefault(position, SandcubeGame.Instance!.Blocks.Air.DefaultBlockState);

    public void SetBlockState(Vector3Int position, BlockState blockState)
    {
        if(position.IsAnyAxis((a, v) => v < 0 || v >= Size.GetAxis(a)))
            throw new ArgumentOutOfRangeException(nameof(position), position, "block position is out of chunk bounds");

        if(GetBlockState(position) == blockState)
            return;

        _blockStates[position] = blockState;
        _meshRebuildRequired = true;
    }


    public override void OnStart()
    {
        if(!GameObject.TryGetComponent(out _modelCollider))
            _modelCollider = GameObject.AddComponent<ModelCollider>();
    }

    public override void Update()
    {
        if(_meshRebuildRequired)
            UpdateModel();
    }

    public virtual void Clear()
    {
        _blockStates.Clear();
        _meshRebuildRequired = true;
    }

    protected virtual void AddVoxelsToMeshBuilder(VoxelMeshBuilder meshBuilder)
    {
        HashSet<Direction> visibleFaces = new();
        var meshes = SandcubeGame.Instance!.BlockMeshes;

        for(int x = 0; x < Size.x; ++x)
        {
            for(int y = 0; y < Size.y; ++y)
            {
                for(int z = 0; z < Size.z; ++z)
                {
                    Vector3Int position = new(x, y, z);
                    var blockState = GetBlockState(position);
                    if(blockState.IsAir())
                        continue;

                    foreach(var direction in Direction.All)
                    {
                        var neighborBlockState = GetBlockState(position + direction);
                        if(neighborBlockState.IsAir() || !neighborBlockState.Block.IsFullBlock(neighborBlockState))
                            visibleFaces.Add(direction);
                        else
                            visibleFaces.Remove(direction);
                    }
                    meshes.BuildAt(meshBuilder, blockState, position * MathV.InchesInMeter, visibleFaces);
                }
            }
        }
    }

    protected virtual void UpdateModelComponents(int requiredCount)
    {
        if(_modelComponents.Count < requiredCount)
        {
            int countToAdd = requiredCount - _modelComponents.Count;
            for(int i = 0; i < countToAdd; ++i)
                _modelComponents.Add(GameObject.AddComponent<ModelComponent>());
        }
        else if(_modelComponents.Count > requiredCount)
        {
            for(int i = _modelComponents.Count - 1; i >= requiredCount; --i)
            {
                GameObject.Components.Remove(_modelComponents[i]);
                _modelComponents.RemoveAt(i);
            }
        }
    }

    protected virtual void UpdateModel()
    {
        _meshRebuildRequired = false;
        VoxelMeshBuilder meshBuilder = new();

        AddVoxelsToMeshBuilder(meshBuilder);

        var buffers = meshBuilder.ToVertexBuffers();
        bool isEmpty = buffers.Count == 0;
        _modelCollider.Enabled = !isEmpty;
        UpdateModelComponents(buffers.Count);

        if(isEmpty)
            return;

        ModelBuilder physicsModelBuilder = new();
        meshBuilder.AddAsCollisionMesh(physicsModelBuilder);
        var model = physicsModelBuilder.Create();
        _modelCollider.Model = model;

        for(int i = 0; i < buffers.Count; ++i)
        {
            ModelBuilder modelBuilder = new();
            Mesh mesh = new(VoxelsMaterial);
            mesh.CreateBuffers(buffers[i]);
            modelBuilder.AddMesh(mesh);
            _modelComponents[i].Model = modelBuilder.Create();

            _modelComponents[i].SceneObject.Attributes.Set("color", SandcubeGame.Instance!.TextureMap.Texture);
        }
    }
}
