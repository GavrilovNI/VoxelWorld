﻿using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.IO;
using Sandcube.Items;
using Sandcube.Meshing;
using Sandcube.Mods.Base;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.Entities;

public class ItemStackEntity : Entity
{
    [Property] protected Rigidbody Rigidbody { get; set; } = null!;
    [Property] protected BoxCollider Collider { get; set; } = null!;

    protected List<ModelRenderer> Renderers { get; set; } = new();

    public Inventories.Stack<Item> ItemStack { get; private set; } = null!;
    public BBox ModelBounds { get; protected set; }


    public static ItemStackEntity Create(Inventories.Stack<Item> itemStack, in EntitySpawnConfig spawnConfig, Vector3 velocity = default)
    {
        ThreadSafe.AssertIsMainThread();
        var entity = (SandcubeBaseMod.Instance!.Entities.ItemStack.CreateEntity(spawnConfig) as ItemStackEntity)!;
        entity.SetItemStack(itemStack);
        entity.Rigidbody.Velocity = velocity;
        return entity;
    }   


    protected override void OnAwake()
    {
        if(ItemStack is null)
            SetItemStack(Inventories.Stack<Item>.Empty);
    }

    public virtual void SetItemStack(Inventories.Stack<Item> itemStack)
    {
        ThreadSafe.AssertIsMainThread();

        ItemStack = itemStack;
        if(itemStack.IsEmpty)
        {
            Collider.Enabled = false;
            foreach(var renderer in Renderers)
                renderer.Destroy();
            Renderers.Clear();
            Rigidbody.Enabled = false;
            ModelBounds = new BBox();
            return;
        }

        var game = SandcubeGame.Instance!;

        var item = ItemStack.Value;
        var itemModel = ItemStack.Value!.Model;

        var isTransparent = item is BlockItem blockItem && blockItem.Block.Properties.IsTransparent;
        var material = isTransparent ? game.TranslucentVoxelsMaterial : game.OpaqueVoxelsMaterial;

        ComplexMeshBuilder visualMeshBuilder = new ComplexMeshBuilder().Add(itemModel);
        for(int i = 0; i < visualMeshBuilder.PartsCount; ++i)
        {
            ModelBuilder modelBuilder = new();
            Mesh mesh = new(material);
            visualMeshBuilder.CreateBuffersFor(mesh, i);
            modelBuilder.AddMesh(mesh);

            var renderer = Components.Create<ModelRenderer>();
            Renderers.Add(renderer);
            renderer.Model = modelBuilder.Create();
        }
        ModelBounds = itemModel.Bounds;

        Collider.Center = itemModel.Bounds.Center;
        Collider.Scale = itemModel.Bounds.Size;
        Collider.Enabled = true;
        Rigidbody.Enabled = true;
    }

    protected override void WriteAdditional(BinaryWriter writer)
    {
        base.WriteAdditional(writer);
        writer.Write(ItemStack ?? Inventories.Stack<Item>.Empty);
        writer.Write(Rigidbody.Velocity);
    }

    protected override void ReadAdditional(BinaryReader reader)
    {
        base.ReadAdditional(reader);
        var itemStack = Inventories.ItemStack.Read(reader);
        SetItemStack(itemStack);
        Rigidbody.Velocity = reader.ReadVector3();
    }

    protected override void DrawGizmos()
    {
        Gizmo.Hitbox.BBox(ModelBounds);
    }
}