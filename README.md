# Ashen Void Boss Framework

This document provides an overview of the ECS (Entity Component System) based framework for creating bosses in the Ashen Void mod.

## Core Concepts

The framework is built on a modern ECS architecture, emphasizing separation of concerns, data-oriented design, and dependency injection.

### Entity, Component, System (ECS)

-   **Entity**: In our case, the `NPC` object is the Entity.
-   **Component**: Components are pure data containers that hold the state of a boss. They should not contain any logic. Examples: `MovementComponent`, `AttackComponent`. All components implement the `IComponent` marker interface.
-   **System**: Systems contain all the logic. They operate on components. The `SystemManager` automatically injects the required components into a system's `Update` method based on its signature. This removes all coupling between systems. Example: `MovementSystem` has an `Update(NPC npc, MovementComponent move)` method.

### The `EcsBoss` Base Class

To create a new boss, you should inherit from `Core.Builders.EcsBoss`. This base class handles all the boilerplate code for setting up the ECS, updating systems, and publishing events.

You only need to implement one abstract method: `InitializeController()`.

### `BossBuilder`

Inside `InitializeController()`, you use the `BossBuilder` to construct your boss. It provides a fluent API to:
-   `AddComponent()`: Register a new component.
-   `AddSystem()`: Register a new system.
-   `WithInitialState()`: Set the initial state for the boss's state machine.

### State Management (FSM)

The framework includes a simple Finite State Machine.
-   **IState**: States define the major behaviors of a boss (e.g., `SpawnState`, `Phase1State`). States should be stateless and retrieve any necessary configuration or data from the `Blackboard`.
-   **StateFactory**: Creates and caches states on demand. States can transition to a new state by calling `aiState.ChangeState<NewState>()`.
-   **Blackboard**: A key-value store that is passed to each state. It's used to share data between systems and states, such as the boss configuration, the player target, etc.

## How to Create a New Boss

1.  **Create a new class** that inherits from `EcsBoss`.
2.  **Implement `SetDefaults()`** as you normally would.
3.  **Implement `InitializeController()`**:
    -   Load your boss's configuration.
    -   Create an `EventBus` and a `StateFactory`.
    -   Use the `BossBuilder` to add all the components and systems your boss needs.
    -   Put any necessary data (like the config object) into the `Blackboard`.
    -   Return the built `ComponentController`.

```csharp
// Example from NightmareCorruption.cs
protected override ComponentController InitializeController()
{
    var configLoader = new ConfigLoader();
    var bossConfig = configLoader.LoadForBoss<BossConfig>(FullName);

    NPC.damage = bossConfig.Damage;
    NPC.defense = bossConfig.Defense;

    EventBus = new EventBus();
    var stateFactory = new StateFactory();

    var controller = new BossBuilder()
        .AddComponent(() => new MovementComponent(NPC, bossConfig.Phase1.Movement))
        .AddComponent(() => new AttackComponent())
        // ... other components and systems
        .AddSystem(new AIBlackboardSystem(EventBus))
        .WithInitialState(typeof(SpawnState))
        .Build();
    
    var aiState = controller.GetComponent<AIStateComponent>();
    aiState?.Blackboard.Set("BossConfig", bossConfig);

    return controller;
}