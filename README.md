# AshenVoid tModLoader Boss 框架 (V2 - 重构版)

这是一个为tModLoader设计的、经过重构的实体-组件-系统（ECS）框架，旨在用于创建复杂的、可维护的、高性能的、网络同步可靠的Boss。

## 核心设计理念

新架构遵循现代数据驱动设计的最佳实践，解决了旧架构在性能、网络和代码结构上的诸多问题。

- **全局ECS世界**: 整个框架由一个在`ModSystem`中运行的、单例的`EcsSystem`驱动。所有的实体（Entity）、组件（Component）和系统（System）都存在于一个统一的`EcsWorld`中，彻底解决了旧架构为每个NPC创建一套独立框架的资源浪费和生命周期管理问题。
- **性能优先**: 系统的更新循环基于**原型（Archetype）**。一个原型代表了一组特定的组件组合。系统只会在包含其所需组件的原型上运行，避免了每帧对大量无关实体的无效检查，从根本上保证了高性能。
- **数据驱动**: 逻辑（Systems）与数据（Components）被严格分离。系统负责行为，组件负责状态。这种分离使得代码功能高度内聚，易于理解、测试和复用。
- **清晰的依赖关系**: 框架完全移除了服务定位器（ServiceLocator），所有模块间的依赖关系都通过构造函数显式注入。这使得代码的依赖关系一目了然，极大地增强了可维护性。
- **可靠的网络同步**: 一个全新的`NetworkManager`负责处理所有网络通信。它通过将单帧内的多个消息打包到同一个网络包中发送，显著降低了网络开销，为实现流畅的多人游戏体验打下了坚实的基础。

## 如何创建一个新Boss

1.  **创建主类**: 创建一个新的NPC类，使其继承自 `Core/Builders/EcsBoss.cs`。
2.  **设置默认值**: 在 `SetBossDefaults()` 方法中，设置NPC的基础属性（如 `NPC.lifeMax`, `NPC.width` 等）。
3.  **构建实体**: 实现 `BuildEntity(EcsWorld world, int entityId)` 方法。这是定义Boss“蓝图”的核心所在。在此方法中：
    - 加载该Boss所需的配置（`BossConfig`）。
    - 创建该Boss所需的所有**组件**（Components），并通过 `world.AddComponent(entityId, ...)` 将它们添加到实体上。
    - **注意**: **系统（Systems）是全局的**，应在 `Core/ECS/EcsSystem.cs` 的 `RegisterGlobalSystems()` 方法中统一注册，而不是在`BuildEntity`中单独添加。
4.  **定义状态**: 为Boss创建不同的FSM状态类（继承自`IState`），并在 `BuildEntity` 方法中将它们注册到 `AIStateComponent` 中。
5.  **实现行为**: （可选）如果某个状态的逻辑比较复杂，可以为其创建一个行为树，并在该状态的 `Update` 方法中驱动它。

这个重构后的框架为您提供了一个稳固的平台。现在，您可以专注于创造富有想象力的Boss行为，而不必为底层的技术实现而烦恼。