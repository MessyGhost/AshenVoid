# AshenVoid Mod

## Overview

AshenVoid is a Terraria mod built with tModLoader. It features a custom Entity-Component-System (ECS) architecture designed to create complex and maintainable boss encounters.

## Core Architecture

The mod's foundation is a data-driven ECS framework that separates logic (Systems) from data (Components). This approach replaces the traditional `ModNPC.AI` hook with a more flexible and scalable design.

### Key Concepts

*   **Entities**: Unique identifiers for game objects (like bosses).
*   **Components**: Pure data containers that describe aspects of an entity (e.g., `HealthComponent`, `MovementComponent`).
*   **Systems**: Global managers that operate on entities with specific sets of components (e.g., `MovementSystem` updates the position of all entities with a `MovementComponent`).
*   **Events**: A robust `EventBus` facilitates communication between systems, both locally and over the network.
*   **Behavior Trees (BT)**: AI logic is defined using Behavior Trees, which are executed by the `BehaviorTreeSystem`. This allows for complex and readable AI patterns.
*   **Finite State Machines (FSM)**: High-level states (e.g., Phase 1, Phase 2) are managed by an FSM, with each state running its own Behavior Tree.

### Networking

The architecture is built with multiplayer in mind.
*   Core logic (AI, movement, attacks) runs exclusively on the server.
*   An `EventBus`-driven networking model synchronizes key actions (`EntityId` assignment, attacks, state changes) to clients.
*   Client-side systems (`AnimationSystem`, `VFXSystem`) listen to these network events to produce visual and audio feedback.

## Current Status

The project has undergone a significant refactoring to fix critical bugs and implement the core gameplay loop.

*   **Infrastructure**: The `EventBus`, `ConfigLoader`, and entity lifecycle management are now stable and reliable.
*   **Networking**: The foundation for multiplayer synchronization is complete.
*   **Gameplay**: The `NightmareCorruption` boss has a basic AI, can target players, move, and perform a basic attack.
*   **Client-Side**: The client can now correctly display animations in response to server-driven attacks.

The project is now in a solid state for future feature development.