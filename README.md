# AshenVoid tModLoader Boss Framework

This project contains a custom ECS (Entity-Component-System) framework designed for creating complex and maintainable bosses in tModLoader.

## Core Concepts

### ECS (Entity-Component-System)
- **Entity**: The NPC itself.
- **Component**: Pure data containers (e.g., `MovementComponent`, `AttackComponent`). They hold state but no logic.
- **System**: Pure logic containers (e.g., `MovementSystem`, `AttackSystem`). They operate on components to perform actions.

### State Management
- **FSM (Finite State Machine)**: The boss's overall behavior is managed by a State Machine (`StateMachine.cs`). Each state (`IState`) represents a major phase or mode (e.g., `Phase1State`, `DeathState`).
- **Behavior Tree**: Within each state, complex attack patterns and decision-making are handled by a Behavior Tree. This allows for modular and readable AI logic.

### Data Flow & Decoupling
The key to this framework is decoupling AI decision-making from execution.
1.  **AI Decision (State/BT)**: The AI's only job is to decide **what** to do. It does this by creating an **Intent** object (e.g., `ChaseIntent`, `ShootProjectileIntent`).
2.  **Blackboard**: The AI places the created `Intent` onto a central `Blackboard`. The `Blackboard` is a simple key-value store for communication.
3.  **System Execution**: Systems (like `MovementSystem`) run every frame. They check the `Blackboard` for relevant `Intents`. If an `Intent` is found, the system executes it by manipulating the data in the corresponding `Component`.

This ensures the AI doesn't need to know *how* to move or attack, only that it *wants* to.

## Creating a Boss
1.  Create a new class that inherits from `EcsBoss`.
2.  Implement the `InitializeController` method.
3.  Use the `BossBuilder` to fluently add components and systems.
4.  Define the boss's behavior by creating `IState` classes.
5.  Use the `WithInitialState` and `WithBlackboardData` methods on the `BossBuilder` to set up the initial state and any required configuration data.

Example from `NightmareCorruption.cs`:
```csharp
var controller = new BossBuilder()
    .AddComponent(() => new MovementComponent(NPC, bossConfig.Phase1.Movement))
    .AddComponent(() => new AttackComponent())
    .AddComponent(() => new HealthComponent(NPC.life))
    .AddSystem(new MovementSystem())
    .AddSystem(new AttackSystem())
    .AddSystem(new HealthSystem())
    .WithInitialState(typeof(SpawnState))
    .WithBlackboardData("BossConfig", bossConfig)
    .OnBuild(c => blackboardSystem.Initialize(EventBus))
    .Build();