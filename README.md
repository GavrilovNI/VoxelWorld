# Voxel World
Voxel World is a 3d game of sandbox and survival genre.
[Game page](https://asset.party/sandcube/voxelworld).

## Features

### Worlds
Currently game can have only 1 world because of game engine restrictions, but that will be changed in the future.
Each world consist of chunks of blocks and can be proceduraly generated using noises and structures.

### World generation
World generation is based on unique seed and generation type. Because of that there are infinte amount of infinite worlds.
Mods can create their own world generators.
![image](https://github.com/GavrilovNI/VoxelWorld/assets/9992453/a2254db1-a42f-4f5d-8539-c8f93c3b8e1a)
    
### World editing
Obviously you can break and place blocks.
   
### Block States
Each block can have different block states. Each block state is represented by block properties that are technically just enums.
When game starts block data is being generated for each block state, so there is no need to calculate it each time on adding block to world.
   
### Block Entities
Block can have a block entity. Block with block entity can store and manipulate custom information per block.
Block entity is a powerfull way to expand block´s abilities, but it can lower game performance.

### Capabilities
There is a capability system, that allows storing stacks of objects(e.g. items).
Blocks entities can have capabilities that can be interacted by other blocks or players.
Player has it's own inventory that can store item stacks. This creates different ways of interacting with world(e.g. pickaxe can break blocks faster when block items allows you to place blocks).

- Player's creative invetory
![image](https://github.com/GavrilovNI/VoxelWorld/assets/9992453/8877f6ec-fda9-47e8-bbc8-947bbf386cab)

- Block's inventory and Player's main inventory
![image](https://github.com/GavrilovNI/VoxelWorld/assets/9992453/39a74064-b9b3-4a16-8f2d-c62e050e7f97)

 
### Crafting
There is a crafting system that allows you craft items and blocks using different ways.
Also you can add your crafting types(by mod) and use them in game.

![image](https://github.com/GavrilovNI/VoxelWorld/assets/9992453/e4b228da-3721-4a78-bde9-dc5db83ef3ab)
![image](https://github.com/GavrilovNI/VoxelWorld/assets/9992453/ee3cb7f4-0b18-4cd8-8c46-f20284a85925)

### Random ticks
Random tick is a block event that randomly occurs for each block.
Blocks can have their own way of handling it(e.g dirt converts to grass when there is grass nearby).

## Game Engine
Game is being developed using [s&box game engine](https://sbox.game/about).

## Modding
Voxel World is being desinged to allow easy way of adding and creating modifications.
Currently s&box is being rewritten and doeasn't allow adding mods(addons) to games. That will be changed in the future and there will be a way to add mods to your world!
