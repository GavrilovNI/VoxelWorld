using Sandcube.Blocks.States;
using Sandcube.Blocks.States.Properties;
using Sandcube.Blocks.States.Properties.Enums;
using Sandcube.Interactions;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Texturing;
using Sandcube.Worlds;
using Sandcube.Meshing;
using Sandcube.Meshing.Blocks;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sandcube.Blocks;

public abstract class TwoPartBlock : Block
{
    public static readonly BlockProperty<TwoPartBlockPartType> PartTypeProperty = new("part_type");

    public IReadOnlyDictionary<Direction, IUvProvider> FirstUvProviders { get; private set; }
    public IReadOnlyDictionary<Direction, IUvProvider> SecondUvProviders { get; private set; }

    [SetsRequiredMembers]
    public TwoPartBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> firstUvProviders,
        IReadOnlyDictionary<Direction, IUvProvider> secondUvProviders) : base(id)
    {
        FirstUvProviders = firstUvProviders;
        SecondUvProviders = secondUvProviders;
    }

    [SetsRequiredMembers]
    public TwoPartBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> uvProviders) : this(id, uvProviders, uvProviders)
    {
    }


    public override IEnumerable<BlockProperty> CombineProperties() => new BlockProperty[] { PartTypeProperty };

    public override BlockState CreateDefaultBlockState(BlockState blockState) => blockState.With(PartTypeProperty, TwoPartBlockPartType.First);


    public abstract Direction GetDirectionToAnotherPart(BlockState blockState);


    public override void OnPlaced(in BlockActionContext context, BlockState firstPartState)
    {
        var secondPartPlaceDirection = GetDirectionToAnotherPart(firstPartState);

        var secondPartState = firstPartState.Change(PartTypeProperty, part => part.GetOpposite());
        var secondPartContext = context + secondPartPlaceDirection;

        bool canReplace = secondPartContext.BlockState.Block.CanBeReplaced(secondPartContext, secondPartState);
        if(!canReplace)
            return;

        bool canStay = secondPartState.Block.CanStay(secondPartContext.World, secondPartContext.Position, secondPartState);
        if(!canStay)
            return;

        _ = context.World.SetBlockState(secondPartContext.Position, secondPartState);
    }

    public override void OnNeighbourChanged(in NeighbourChangedContext context)
    {
        var secondPartPosition = context.ThisPosition + GetDirectionToAnotherPart(context.ThisBlockState);
        if(context.NeighbourPosition != secondPartPosition)
            return;

        bool isMyPart = context.NeighbourNewBlockState.Block == this &&
            GetDirectionToAnotherPart(context.NeighbourNewBlockState) == context.DirectionToNeighbour.GetOpposite();

        BlockState newBlockState = isMyPart ? context.NeighbourNewBlockState.Change(PartTypeProperty, part => part.GetOpposite()) : BlockState.Air;
        context.World.SetBlockState(context.ThisPosition, newBlockState);
    }


    public override ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState)
    {
        var uvProviders = blockState.GetValue(PartTypeProperty) == TwoPartBlockPartType.First ? FirstUvProviders : SecondUvProviders;
        return VisualMeshes.FullBlock.Make(uvProviders.ToDictionary(p => p.Key, p => p.Value.Uv));
    }
    public override ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
    public override ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
}
