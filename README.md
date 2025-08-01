# AshenVoid tModLoader Boss Framework

This project contains a custom ECS (Entity-Component-System) framework designed for creating complex, maintainable, and multiplayer-compatible bosses in tModLoader.

## Core Concepts

### ECS (Entity-Component-System)
- **Entity**: The `ModNPC` itself, represented by the `EcsBoss` base class.
- **Component**: Pure data containers (e.g., `MovementComponent`, `AttackComponent`). They hold state but no logic.
- **System**: Pure logic containers (e.g., `MovementSystem`, `AttackSystem`). They operate on components to perform actions.

### State Management & Network Sync
- **FSM (Finite State Machine)**: The boss's overall behavior is managed by a `StateMachine`. Each `IState` represents a major phase (e.g., `Phase1State`, `DeathState`).
- **State Synchronization**: The FSM's current state is automatically synchronized from the server to clients using `npc.ai[]`. The server has authority over the state, and clients react to state changes.
- **Behavior Tree**: Within each state, complex attack patterns and decision-making can be handled by a Behavior Tree.

### Data Flow & Decoupling
The framework decouples AI decision-making from execution, which is crucial for clarity and network compatibility.

1.  **AI Decision (Server-Side)**: The AI (FSM/BT) decides **what** to do. It creates an **Intent** object (e.g., `ChaseIntent`, `ShootProjectileIntent`).
2.  **Blackboard**: The AI places the `Intent` onto a central `Blackboard` (a key-value store).
3.  **System Execution (Server-Side)**: Systems like `MovementSystem` and `AttackSystem` run on the server. They read `Intents` from the `Blackboard` and execute them by manipulating component data and game state.
4.  **State & Position Sync (tModLoader)**: tModLoader automatically syncs the `npc.ai[]` array (which we use for state) and `npc.position`/`npc.velocity` to clients.
5.  **System Execution (Client-Side)**: Systems like `AnimationSystem` and `VFXSystem` run on the client, reacting to the synchronized state to produce visual and audio feedback.

This ensures the AI logic runs only on the server, and clients are just "puppets" that render the outcome.

## Creating a Boss

1.  Create a new class that inherits from `EcsBoss`.
2.  Implement `SetBossDefaults()` to configure standard `NPC` properties (`width`, `height`, `lifeMax`, etc.).
3.  Implement `InitializeController()`. This method is called **once** from `SetDefaults` to create the boss's "blueprint".
4.  Inside `InitializeController`, use the `BossBuilder` to add all necessary components and systems.
5.  **Crucially**, register all of your `IState` classes with the `AIStateComponent` to get unique network IDs. The order of registration matters!
6.  Define the boss's behavior by creating `IState` classes.

Example from the refactored `NightmareCorruption.cs`:
```csharp
// In InitializeController()
var aiStateComponent = new AIStateComponent(NPC, stateFactory);

// Register states for network IDs. SpawnState gets ID 0, Phase1State gets ID 1, etc.
aiStateComponent.RegisterState<SpawnState>();
aiStateComponent.RegisterState<Phase1State>();
aiStateComponent.RegisterState<DeathState>();

var builder = new BossBuilder()
    .AddComponent(() => new MovementComponent(NPC, bossConfig.Phase1.Movement))
    .AddComponent(() => new AttackComponent())
    .AddComponent(() => aiStateComponent) // Add the pre-configured component
    .AddSystem(new MovementSystem()) // Runs on Server
    .AddSystem(new AttackSystem())   // Runs on Server
    .AddSystem(new AnimationSystem()); // Runs on Both

return builder.Build();
```

## System Execution Side

Each system must implement the `ExecutionSide` property:
- `SystemExecutionSide.Server`: For all logic, AI, and gameplay calculations.
- `SystemExecutionSide.Client`: For purely visual effects that don't depend on gameplay state.
- `SystemExecutionSide.Both`: For logic that needs to run everywhere, like animations.

The `SystemManager` will automatically ensure systems only run on the correct side.