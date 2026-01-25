# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Vintage Story mod that extends the functionality of locusts - small spider-like automatons from the game's lore that historically assisted humanity with logistics tasks. The mod enables locusts to work together in coordinated "hives" to manage inventory and automate item transfer between storage locations.

## Environment Setup

**Required Environment Variable:**
- `VINTAGE_STORY`: Must point to your Vintage Story installation directory containing the game DLLs (VintagestoryAPI.dll, VintagestoryLib.dll, etc.) and mods

**Build System:**
The project uses Cake Build (Frosting) for compilation and packaging. The CakeBuild project orchestrates the entire build pipeline.

## Build Commands

All build commands are run from the `CakeBuild` directory:

```bash
cd CakeBuild
dotnet run -- [options]
```

**Build targets:**
- `dotnet run` - Default target: validates JSON, builds, and packages the mod
- `dotnet run --target=Build` - Build only (validates JSON + compiles)
- `dotnet run --target=ValidateJson` - Validate all JSON files in assets/
- `dotnet run --configuration=Debug` - Build in Debug mode (default is Release)
- `dotnet run --skipJsonValidation=true` - Skip JSON validation

**Output:**
- Compiled mod: `LocustLogistics/bin/{Configuration}/Mods/mod/`
- Packaged mod: `Releases/{modid}_{version}.zip`

## Development Workflow

**Running the mod:**
Use Visual Studio launch profiles defined in `LocustLogistics/Properties/launchSettings.json`:
- "Client" profile: Launches game client with mod loaded
- "Server" profile: Launches dedicated server with mod loaded

Both profiles use `--addModPath` and `--addOrigin` to load the mod directly from the build output without packaging.

**Standard C# build:**
You can also build directly with:
```bash
dotnet build LocustLogistics/LocustLogistics.csproj
```

## Architecture

The architecture uses a **decentralized, event-driven design** where hive membership is managed by a core system, with specialized subsystems tracking their own member types independently.

### Core Hive System (Tuning/)

**TuningSystem** - Central hive membership coordinator
- Manages hive IDs (integer-based) via `MembershipRegistry<IHiveMember>`
- Provides `Tune(member, hiveId)` to add/remove members from hives
- Provides `CreateHive()` to generate new unique hive IDs
- Persists next hive ID in save game to prevent ID conflicts
- Calls `IHiveMember.OnTuned` event when membership changes

**IHiveMember** - Minimal core interface for hive membership
- Provides `OnTuned` event: `Action<int?, int?>` (previousHive, newHive)
- Blocks and entities implement this through behaviors

**BEBehaviorLocustHiveTunable** - BlockEntity behavior for hive membership
- Implements `IHiveMember` for any block
- Auto-creates a new hive on placement if not already tuned
- Serializes hive ID via `ToTreeAttributes/FromTreeAttributes`
- Fires `OnTuned` event when hive changes (consumed by specialized behaviors)

**EntityBehaviorHiveTunable** - Entity behavior for hive membership
- Implements `IHiveMember` for any entity
- Provides the same functionality as BEBehaviorLocustHiveTunable for entities

**ItemHiveTuner** - In-game tool for managing hive membership
- Modes: Calibrate (select source hive), Tune (add to calibrated hive), Detune (remove from hive)
- Works with any `IHiveMember` (blocks or entities)
- Saves mode state and calibrated hive in itemstack attributes

### Specialized Subsystems

**NestsSystem** (Nests/)
- Manages `MembershipRegistry<ILocustNest>` to track nest members per hive
- Provides `UpdateNestMembership(nest, hiveId)` to update nest membership
- Registers `BEBehaviorHiveLocustNest` and `AiTaskReturnToNest`
- Dynamically patches hacked locust entities to add `returnToNest` AI task via `AssetsFinalize`

**BEBehaviorHiveLocustNest** - Nest behavior for storing locusts
- Implements `ILocustNest`
- Serializes locust entities to byte arrays for storage
- Supports store/unstore operations with capacity limits
- Releases all stored locusts when nest is removed
- Listens to `BEBehaviorLocustHiveTunable.OnTuned` to update NestsSystem membership

**LogisticsSystem** (Logistics/)
- Manages two registries: `MembershipRegistry<ILogisticsWorker>` and `MembershipRegistry<ILogisticsStorage>`
- Provides `UpdateLogisticsWorkerMembership(worker, hiveId)` and `UpdateLogisticsStorageMembership(storage, hiveId)`
- Creates and manages `LogisticsNetwork` instances per hive ID
- Provides `GetNetworkFor(hiveId)` to access networks
- Registers behaviors: `BEBehaviorHiveAccessPort`, `BEBehaviorHivePushBeacon`, `EntityBehaviorLocustLogisticsWorker`
- Dynamically patches hacked locust entities to add `doLogisticsAccessTasks` AI task via `AssetsFinalize`
- Runs game tick listener to process queued logistics promises (max 10 per network per tick)

**BEBehaviorHiveAccessPort** - Access port for inventories (formerly "storage beacon")
- Implements `ILogisticsStorage`
- Provides access to the inventory of the block in the direction it faces
- Uses `BlockFaceAccessible` as access method (locusts access from opposite face)
- Manages `LogisticsReservation` objects to prevent double-booking of items
- Implements `TryReserve(stack)` for reservation system (stack sign indicates direction)
- Listens to `BEBehaviorLocustHiveTunable.OnTuned` to update LogisticsSystem membership

**BEBehaviorHivePushBeacon** - Push beacon for automated item transfer
- Manages push requests by creating `LogisticsPromise` objects via network
- `PushAll()` creates push promises for all items in attached storage
- Tracks active promises and cancels them on block removal/unload
- Requires attached storage to have hive membership to function

**EntityBehaviorLocustLogisticsWorker** - Worker behavior for logistics tasks
- Implements `ILogisticsWorker`
- Tracks assigned `WorkerEffort` (wraps a logistics promise)
- Listens to `EntityBehaviorHiveTunable.OnTuned` to update LogisticsSystem membership

**LogisticsNetwork** (Systems/Logistics/Core/)
- Coordinates logistics operations within a single hive
- Manages `QueuedPromises` and worker assignments
- Provides `Push(stack, source)` - negates stack size to indicate Take from source
- Provides `Pull(stack, into)` - keeps positive stack size to indicate Give to target
- `CommisionWorkersForNextQueuedPromise()` assigns workers to queued promises
- Uses reservation system to prevent conflicts

**AiTaskLocustLogisticsOperation** - AI task for executing logistics operations
- Registered by LogisticsSystem as "doLogisticsAccessTasks"
- Executes worker efforts using pathfinding and inventory operations
- Handles item pickup and delivery to storage access points

## Key Design Patterns

**Decentralized Membership Tracking:** Instead of a central `LocustHive` object, each specialized ModSystem maintains its own `MembershipRegistry<T>` to track members by hive ID. The `MembershipRegistry` class (in Systems/Membership/) provides:
- `AssignMembership(member, hiveId)` - Add/remove/change membership, returns previous hive ID
- `GetMembersOf(hiveId)` - Get all members of a hive as `IEnumerable<T>`
- `GetMembershipOf(member, out hiveId)` - Check if member belongs to a hive

**Event-Driven Updates:** Uses a layered event system:
1. `TuningSystem` calls `IHiveMember.OnTuned` when membership changes
2. `BEBehaviorLocustHiveTunable`/`EntityBehaviorHiveTunable` fire their own `OnTuned` events
3. Specialized behaviors (nest, storage, worker) listen to these events and update their ModSystems

**Behavior Composition:** Blocks/entities gain hive capabilities by combining behaviors:
- `BEBehaviorLocustHiveTunable` (core membership) + `BEBehaviorHiveLocustNest` (nest features)
- `BEBehaviorLocustHiveTunable` (core membership) + `BEBehaviorHiveAccessPort` (storage features)
- `EntityBehaviorHiveTunable` (core membership) + `EntityBehaviorLocustLogisticsWorker` (worker features)

**Persistence:** Each behavior serializes its own state. Hive ID is serialized by the tunable behaviors and restored on load.

**Runtime Entity Patching:** Both `NestsSystem` and `LogisticsSystem` use JSON patching in `AssetsFinalize` to dynamically inject AI tasks into existing entity definitions without modifying base game assets. This only runs server-side to prevent double-patching.

**Reservation System:** The logistics system uses `LogisticsReservation` objects to prevent double-booking of inventory slots. When a worker is assigned to move items, the source/destination slots are reserved until the operation completes or is cancelled.

**Promise-Based Task Management:** Logistics operations use `LogisticsPromise` objects that represent pending work. Promises can be queued, fulfilled, or cancelled. Workers are assigned `WorkerEffort` objects that wrap promises and track execution state.

**Stack Size Sign Convention:** The logistics system uses stack size sign to indicate operation direction instead of a separate enum:
- **Positive stack size** = Give (storage receives items)
- **Negative stack size** = Take (storage provides items)
- **Zero stack size** = No-op (operations return null)

This convention is used throughout `LogisticsPromise`, `LogisticsReservation`, `IStorageAccessMethod.CanDo()`, `ILogisticsStorage.TryReserve()`, and `ILogisticsWorker.GetEfforts()`. The `Push()` method negates the stack since it takes FROM the source, while `Pull()` keeps positive since it gives TO the target.

## Code Structure

**Game/** - Game-facing implementations (blocks, entities, items, behaviors, AI tasks)
- `Tuning/` - Core hive membership (TuningSystem, IHiveMember, tunable behaviors, ItemHiveTuner)
- `Nests/` - Nest system (NestsSystem, nest behaviors, return to nest AI)
- `Logistics/` - Logistics system (LogisticsSystem, access ports, push beacons, worker behaviors, logistics AI)
- `Util/` - Extension methods and utilities

**Systems/** - Internal logic and data structures (not directly game-facing)
- `Membership/` - `MembershipRegistry<T>` and `IMembershipRegistry<T>` for tracking hive memberships
- `Nests/` - `ILocustNest` interface
- `Logistics/Core/` - Core logistics abstractions (`ILogisticsNetwork`, `ILogisticsStorage`, `ILogisticsWorker`, `LogisticsNetwork`, `LogisticsPromise`, `LogisticsReservation`, `WorkerEffort`)
- `Logistics/AccessMethods/` - Access method implementations (e.g., `BlockFaceAccessible`)

## Assets Structure

- `assets/locustlogistics/itemtypes/tools/` - Item definitions (hive-tuner.json)
- `assets/locustlogistics/blocktypes/` - Block definitions
- `assets/locustlogistics/lang/` - Localization files (en.json)

Asset JSON files are validated during the build process unless `--skipJsonValidation=true` is passed.

## Dependencies

The mod references Vintage Story assemblies (all set to `<Private>false</Private>` to avoid copying):
- VintagestoryAPI.dll - Core API
- VintagestoryLib.dll - Implementation library
- VSSurvivalMod.dll - Survival mode features
- VSEssentials.dll - Essential features (WaypointsTraverser, etc.)
- VSCreativeMod.dll - Creative mode features
- 0Harmony.dll - Harmony patching library

## Current State

**Fully Implemented:**
- Core hive membership system with integer-based hive IDs via TuningSystem
- Hive tuner tool for adding/removing members from hives
- Block and entity behaviors for hive membership
- Nest system with locust storage/unstorage functionality
- Return to nest AI task
- Access port system for marking inventories as hive-accessible
- Push beacon system for automated item transfer
- Logistics network with promise-based task coordination
- Reservation system for inventory operations
- Worker assignment and logistics AI task execution
- Event-driven architecture connecting all subsystems

**In Progress:**
- Pull beacon for requesting specific items
- Storage filtering and smart storage settings GUI
- Hive gauge for monitoring hive status
- Advanced logistics operations (multi-step transfers, prioritization)

## Development Guidelines

- **Avoid unnecessary wrapper functions** - Only create functions when they prevent significant code duplication
- **Vintage Story source reference** - The Vintage Story source code is available at `C:\Users\Adam\source\repos\vintagestory` with survival, creative, and essentials mods, public API, and decompiled engine