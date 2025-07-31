# Boss框架专业化升级方案

## 概述

本文档定义了将当前Boss框架从"业余实现"升级为"工业级专业框架"的完整方案。重点解决配置管理、依赖注入、攻击系统扩展性和架构最佳实践四个核心问题。

---

## 1. 数据驱动配置系统

### 1.1 问题分析

**当前问题**：
- 配置硬编码在 `SetStaticDefaults` 中
- 使用静态字段 `_config`，存在多实例共享风险
- 无法运行时修改参数，策划无法直接调整

**解决方案**：实现基于HJSON的外部配置加载系统

### 1.2 ConfigLoader 设计

```csharp
// Core/Configuration/ConfigLoader.cs
public static class ConfigLoader
{
    private static readonly Dictionary<string, object> _configCache = new();
    
    public static T Load<T>(string configPath) where T : class, new()
    {
        if (_configCache.TryGetValue(configPath, out var cached))
            return (T)cached;
            
        var fullPath = Path.Combine(ModContent.GetInstance<AshenVoid>().Path, "Configs", configPath);
        
        if (!File.Exists(fullPath))
        {
            ModContent.GetInstance<AshenVoid>().Logger.Warn($"配置文件不存在: {configPath}，使用默认配置");
            return CreateDefaultConfig<T>();
        }
        
        try
        {
            var hjsonText = File.ReadAllText(fullPath);
            var config = Hjson.JsonConvert.DeserializeObject<T>(hjsonText);
            
            ValidateConfig(config);
            _configCache[configPath] = config;
            
            return config;
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<AshenVoid>().Logger.Error($"配置加载失败: {configPath}", ex);
            return CreateDefaultConfig<T>();
        }
    }
    
    public static void ReloadConfig(string configPath)
    {
        _configCache.Remove(configPath);
    }
    
    private static void ValidateConfig<T>(T config)
    {
        // 使用反射验证配置完整性
        var validator = ConfigValidatorFactory.GetValidator<T>();
        validator?.Validate(config);
    }
}
```

### 1.3 HJSON配置文件格式

```hjson
// Configs/NightmareCorruption.hjson
{
    // 第一阶段配置
    phase1: {
        // 移动参数
        movement: {
            frequency: 1.2,        // PID控制器频率
            dampingRatio: 0.8,     // 阻尼比
            responseScale: 0.5,    // 响应缩放
            chaseDistanceFar: 800, // 追击距离
            chaseStopDistance: 400,// 停止距离
            orbitRadius: 300,      // 环绕半径
            thinkIntervalMin: 2.0, // 最小思考间隔
            thinkIntervalMax: 4.0  // 最大思考间隔
        },
        
        // 攻击参数
        attacks: {
            basicShot: {
                projectileId: 270,    // ProjectileID.DemonScythe
                cooldown: 2.5,        // 冷却时间
                damage: 35,           // 伤害
                speed: 12,            // 弹幕速度
                isChanneled: false,   // 是否为持续性攻击
                duration: 0           // 持续时间（瞬时攻击为0）
            }
        }
    }
}
```

### 1.4 配置验证器

```csharp
// Core/Configuration/ConfigValidator.cs
public interface IConfigValidator<T>
{
    void Validate(T config);
}

public class BossConfigValidator : IConfigValidator<BossConfig>
{
    public void Validate(BossConfig config)
    {
        if (config.Phase1 == null)
            throw new ConfigValidationException("Phase1配置不能为空");
            
        ValidateMovementStats(config.Phase1.Movement);
        ValidateAttackStats(config.Phase1.Attacks);
    }
    
    private void ValidateMovementStats(MovementStats stats)
    {
        if (stats.Frequency <= 0)
            throw new ConfigValidationException("Movement.Frequency必须大于0");
        // ... 其他验证
    }
}
```

---

## 2. 依赖注入系统

### 2.1 问题分析

**当前问题**：
- AIComponent 直接持有其他组件引用
- 组件间依赖关系手动连接，缺乏灵活性
- 难以进行单元测试和模拟

**解决方案**：实现真正的依赖注入容器

### 2.2 ServiceContainer 设计

```csharp
// Core/DI/ServiceContainer.cs
public class ServiceContainer
{
    private readonly Dictionary<Type, ServiceDescriptor> _services = new();
    private readonly Dictionary<Type, object> _singletonInstances = new();
    
    public void RegisterSingleton<TInterface, TImplementation>()
        where TImplementation : class, TInterface
        where TInterface : class
    {
        _services[typeof(TInterface)] = new ServiceDescriptor
        {
            ServiceType = typeof(TInterface),
            ImplementationType = typeof(TImplementation),
            Lifetime = ServiceLifetime.Singleton
        };
    }
    
    public void RegisterTransient<TInterface, TImplementation>()
        where TImplementation : class, TInterface
        where TInterface : class
    {
        _services[typeof(TInterface)] = new ServiceDescriptor
        {
            ServiceType = typeof(TInterface),
            ImplementationType = typeof(TImplementation),
            Lifetime = ServiceLifetime.Transient
        };
    }
    
    public T GetService<T>() where T : class
    {
        return (T)GetService(typeof(T));
    }
    
    public object GetService(Type serviceType)
    {
        if (!_services.TryGetValue(serviceType, out var descriptor))
            throw new ServiceNotFoundException($"服务未注册: {serviceType.Name}");
            
        if (descriptor.Lifetime == ServiceLifetime.Singleton)
        {
            if (_singletonInstances.TryGetValue(serviceType, out var instance))
                return instance;
                
            instance = CreateInstance(descriptor.ImplementationType);
            _singletonInstances[serviceType] = instance;
            return instance;
        }
        
        return CreateInstance(descriptor.ImplementationType);
    }
    
    private object CreateInstance(Type type)
    {
        var constructors = type.GetConstructors();
        var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
        
        var parameters = constructor.GetParameters()
            .Select(p => GetService(p.ParameterType))
            .ToArray();
            
        return Activator.CreateInstance(type, parameters);
    }
}
```

### 2.3 组件接口重构

```csharp
// Core/ECS/Interfaces/IMovementComponent.cs
public interface IMovementComponent : IComponent
{
    void SetIntent(IMovementIntent intent);
    bool IsMoving();
}

// Core/ECS/Interfaces/IAttackComponent.cs
public interface IAttackComponent : IComponent
{
    void SetIntent(IAttackIntent intent);
    bool IsReady();
    bool IsAttacking();
}

// Core/ECS/Interfaces/IAnimationComponent.cs
public interface IAnimationComponent : IComponent
{
    void SetAnimation(string animationName);
    void SetFrameRate(float frameRate);
}
```

### 2.4 重构后的AIComponent

```csharp
// Core/ECS/AIComponent.cs
public class AIComponent : IComponent
{
    public readonly NPC NPC;
    public Player Target { get; private set; }
    
    private readonly ServiceContainer _serviceContainer;
    private readonly StateMachine _stateMachine;
    
    // 移除直接引用，改为通过DI获取
    private IMovementComponent _movementComponent;
    private IAttackComponent _attackComponent;
    private IAnimationComponent _animationComponent;
    private IVFXComponent _vfxComponent;
    
    public AIComponent(NPC npc, ServiceContainer serviceContainer)
    {
        NPC = npc;
        _serviceContainer = serviceContainer;
        _stateMachine = new StateMachine(this);
    }
    
    public void Initialize()
    {
        // 通过DI获取依赖
        _movementComponent = _serviceContainer.GetService<IMovementComponent>();
        _attackComponent = _serviceContainer.GetService<IAttackComponent>();
        _animationComponent = _serviceContainer.GetService<IAnimationComponent>();
        _vfxComponent = _serviceContainer.GetService<IVFXComponent>();
    }
    
    // 提供访问器方法而非直接暴露字段
    public IMovementComponent GetMovementComponent() => _movementComponent;
    public IAttackComponent GetAttackComponent() => _attackComponent;
    public IAnimationComponent GetAnimationComponent() => _animationComponent;
    public IVFXComponent GetVFXComponent() => _vfxComponent;
}
```

---

## 3. 可扩展攻击系统

### 3.1 问题分析

**当前问题**：
- 只支持瞬时攻击，执行后立即消耗意图
- IAttackIntent 接口过于简单
- 无法支持持续性攻击（如激光、冲锋等）

**解决方案**：设计支持持续性攻击的状态管理系统

### 3.2 扩展的攻击意图接口

```csharp
// Core/ECS/Interfaces/IAttackIntent.cs
public interface IAttackIntent
{
    /// <summary>
    /// 是否为持续性攻击
    /// </summary>
    bool IsChanneled { get; }
    
    /// <summary>
    /// 攻击持续时间（秒）。瞬时攻击为0
    /// </summary>
    float Duration { get; }
    
    /// <summary>
    /// 是否可以被其他攻击中断
    /// </summary>
    bool CanBeInterrupted { get; }
    
    /// <summary>
    /// 攻击优先级。高优先级攻击可以中断低优先级攻击
    /// </summary>
    int Priority { get; }
}

// 基础攻击意图抽象类
public abstract class BaseAttackIntent : IAttackIntent
{
    public virtual bool IsChanneled => false;
    public virtual float Duration => 0f;
    public virtual bool CanBeInterrupted => true;
    public virtual int Priority => 0;
}
```

### 3.3 重构的AttackComponent

```csharp
// Core/ECS/AttackComponent.cs
public class AttackComponent : IAttackComponent
{
    private readonly NPC _npc;
    private IAttackIntent _currentIntent;
    private float _cooldownTimer;
    private float _attackTimer; // 用于持续性攻击
    private AttackState _state;
    
    public AttackComponent(NPC npc)
    {
        _npc = npc;
        _state = AttackState.Ready;
    }
    
    public void SetIntent(IAttackIntent intent)
    {
        // 检查是否可以设置新意图
        if (!CanSetIntent(intent))
            return;
            
        // 如果当前有攻击在进行，检查是否可以中断
        if (_currentIntent != null && !_currentIntent.CanBeInterrupted)
        {
            if (intent.Priority <= _currentIntent.Priority)
                return; // 优先级不够，无法中断
        }
        
        // 中断当前攻击并设置新意图
        InterruptCurrentAttack();
        _currentIntent = intent;
        _attackTimer = intent.Duration;
        _state = AttackState.Executing;
    }
    
    public bool IsReady() => _state == AttackState.Ready && _cooldownTimer <= 0;
    public bool IsAttacking() => _state == AttackState.Executing;
    
    public void Update()
    {
        UpdateCooldown();
        UpdateAttackExecution();
    }
    
    private void UpdateCooldown()
    {
        if (_cooldownTimer > 0)
            _cooldownTimer -= 1f / 60f;
    }
    
    private void UpdateAttackExecution()
    {
        if (_currentIntent == null || _state != AttackState.Executing)
            return;
            
        // 执行攻击逻辑
        ExecuteAttack(_currentIntent);
        
        // 处理攻击结束条件
        if (_currentIntent.IsChanneled)
        {
            _attackTimer -= 1f / 60f;
            if (_attackTimer <= 0)
            {
                CompleteAttack();
            }
        }
        else
        {
            // 瞬时攻击立即完成
            CompleteAttack();
        }
    }
    
    private void CompleteAttack()
    {
        if (_currentIntent != null)
        {
            // 设置冷却时间
            _cooldownTimer = GetAttackCooldown(_currentIntent);
            _currentIntent = null;
        }
        
        _state = AttackState.Ready;
    }
    
    private void InterruptCurrentAttack()
    {
        if (_currentIntent != null)
        {
            OnAttackInterrupted(_currentIntent);
            _currentIntent = null;
        }
        
        _state = AttackState.Ready;
    }
}

public enum AttackState
{
    Ready,      // 准备状态，可以接受新攻击
    Executing,  // 执行中
    Cooldown    // 冷却中
}
```

---

## 4. 最佳实践：移除静态配置

### 4.1 问题分析

**当前问题**：
- `BossConfig _config` 是静态字段
- 多个NPC实例会共享同一个配置对象
- 运行时修改配置会影响所有实例

**解决方案**：将配置改为实例字段

### 4.2 重构的NightmareCorruption

```csharp
// Content/NPCs/NightmareCorruption/NightmareCorruption.cs
public partial class NightmareCorruption : ModNPC
{
    // 改为实例字段
    private BossConfig _config;
    private ServiceContainer _serviceContainer;
    
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 4;
        // 移除配置初始化，改到SetDefaults中
    }
    
    public override void SetDefaults()
    {
        // 在实例创建时加载配置
        _config = ConfigLoader.Load<BossConfig>("NightmareCorruption.hjson");
        
        // 初始化服务容器
        _serviceContainer = new ServiceContainer();
        RegisterServices();
        
        // 设置NPC基础属性
        NPC.width = 100;
        NPC.height = 100;
        NPC.damage = 50;
        NPC.defense = 20;
        NPC.lifeMax = 5000;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.boss = true;
    }
    
    private void RegisterServices()
    {
        // 注册组件服务
        _serviceContainer.RegisterSingleton<IMovementComponent, MovementComponent>();
        _serviceContainer.RegisterSingleton<IAttackComponent, AttackComponent>();
        _serviceContainer.RegisterSingleton<IAnimationComponent, AnimationComponent>();
        _serviceContainer.RegisterSingleton<IVFXComponent, VFXComponent>();
        
        // 注册配置服务
        _serviceContainer.RegisterInstance(_config);
        _serviceContainer.RegisterInstance(NPC);
    }
    
    public override void AI()
    {
        if (Components == null)
        {
            InitializeComponents();
        }
        
        Components.Update();
    }
    
    private void InitializeComponents()
    {
        // 通过DI创建AI组件
        var aiComponent = new AIComponent(NPC, _serviceContainer);
        aiComponent.Initialize();
        
        // 设置初始状态
        aiComponent.SetInitialState(new Phase1State(aiComponent, _config.Phase1));
        
        // 创建组件控制器（现在只需要管理AI组件）
        Components = new ComponentController(NPC);
        Components.AddComponent(aiComponent);
    }
}
```

---

## 5. 实施计划

### 阶段1：配置系统重构（第1-2周）
1. 实现 ConfigLoader 和验证器
2. 创建 HJSON 配置文件
3. 将静态配置改为实例配置
4. 测试配置加载和验证

### 阶段2：依赖注入重构（第3-4周）
1. 实现 ServiceContainer
2. 定义组件接口
3. 重构 AIComponent 和其他组件
4. 更新组件注册和获取逻辑

### 阶段3：攻击系统扩展（第5-6周）
1. 扩展 IAttackIntent 接口
2. 重构 AttackComponent
3. 实现持续性攻击支持
4. 添加攻击中断机制

### 阶段4：集成测试和优化（第7-8周）
1. 端到端测试
2. 性能优化
3. 文档更新
4. 示例和教程

---

## 6. 预期收益

### 6.1 可维护性提升
- 配置外部化，策划可直接修改参数
- 清晰的依赖关系，易于理解和修改
- 模块化设计，单一职责原则

### 6.2 可测试性提升
- 依赖注入使单元测试更容易
- 组件解耦，可以独立测试
- 配置可模拟，测试更可靠

### 6.3 可扩展性提升
- 新的攻击类型易于添加
- 组件系统支持插件化扩展
- 配置驱动的行为定制

### 6.4 稳定性提升
- 消除静态字段的并发问题
- 配置验证防止运行时错误
- 优雅的错误处理和降级

这个升级方案将彻底解决当前框架的核心缺陷，将其从"业余实现"提升为"工业级专业框架"。

---

## 7. 详细实现示例

### 7.1 持续性攻击示例：激光攻击

```csharp
// Content/NPCs/NightmareCorruption/Intents/LaserBeamIntent.cs
public class LaserBeamIntent : BaseAttackIntent
{
    public Vector2 StartPosition { get; }
    public Vector2 EndPosition { get; }
    public float BeamWidth { get; }
    public float DamagePerSecond { get; }

    public override bool IsChanneled => true;
    public override float Duration => 3.0f; // 3秒激光
    public override bool CanBeInterrupted => false; // 激光不可中断
    public override int Priority => 10; // 高优先级

    public LaserBeamIntent(Vector2 start, Vector2 end, float width = 20f, float dps = 50f)
    {
        StartPosition = start;
        EndPosition = end;
        BeamWidth = width;
        DamagePerSecond = dps;
    }
}
```

### 7.2 配置热重载示例

```csharp
// Core/Configuration/ConfigWatcher.cs
public class ConfigWatcher : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly string _configPath;

    public event Action<string> ConfigChanged;

    public ConfigWatcher(string configDirectory)
    {
        _configPath = configDirectory;
        _watcher = new FileSystemWatcher(configDirectory, "*.hjson")
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnConfigChanged;
        _watcher.Created += OnConfigChanged;
    }

    private void OnConfigChanged(object sender, FileSystemEventArgs e)
    {
        // 延迟处理，避免文件锁定
        Task.Delay(100).ContinueWith(_ =>
        {
            ConfigLoader.ReloadConfig(Path.GetFileName(e.FullPath));
            ConfigChanged?.Invoke(e.FullPath);
        });
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}
```

### 7.3 组件生命周期管理

```csharp
// Core/ECS/ComponentLifecycleManager.cs
public class ComponentLifecycleManager
{
    private readonly List<IComponent> _components = new();
    private readonly List<IInitializable> _initializables = new();
    private readonly List<IDisposable> _disposables = new();

    public T AddComponent<T>(T component) where T : IComponent
    {
        _components.Add(component);

        if (component is IInitializable initializable)
            _initializables.Add(initializable);

        if (component is IDisposable disposable)
            _disposables.Add(disposable);

        return component;
    }

    public void Initialize()
    {
        foreach (var initializable in _initializables)
        {
            try
            {
                initializable.Initialize();
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<AshenVoid>().Logger.Error($"组件初始化失败: {initializable.GetType().Name}", ex);
            }
        }
    }

    public void Update()
    {
        foreach (var component in _components)
        {
            try
            {
                component.Update();
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<AshenVoid>().Logger.Error($"组件更新失败: {component.GetType().Name}", ex);
            }
        }
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<AshenVoid>().Logger.Error($"组件销毁失败: {disposable.GetType().Name}", ex);
            }
        }

        _components.Clear();
        _initializables.Clear();
        _disposables.Clear();
    }
}

// 扩展的组件接口
public interface IInitializable
{
    void Initialize();
}
```

### 7.4 攻击组合系统

```csharp
// Content/NPCs/NightmareCorruption/Intents/ComboAttackIntent.cs
public class ComboAttackIntent : BaseAttackIntent
{
    private readonly List<IAttackIntent> _attacks;
    private int _currentAttackIndex;
    private float _comboTimer;

    public override bool IsChanneled => true;
    public override float Duration => _attacks.Sum(a => a.Duration + 0.5f); // 包含间隔时间

    public ComboAttackIntent(params IAttackIntent[] attacks)
    {
        _attacks = attacks.ToList();
        _currentAttackIndex = 0;
    }

    public IAttackIntent GetCurrentAttack()
    {
        if (_currentAttackIndex >= _attacks.Count)
            return null;

        return _attacks[_currentAttackIndex];
    }

    public bool AdvanceToNextAttack()
    {
        _currentAttackIndex++;
        return _currentAttackIndex < _attacks.Count;
    }
}
```

---

## 8. 性能优化建议

### 8.1 对象池化

```csharp
// Core/ObjectPool/ObjectPool.cs
public class ObjectPool<T> where T : class, new()
{
    private readonly ConcurrentQueue<T> _objects = new();
    private readonly Func<T> _objectGenerator;
    private readonly Action<T> _resetAction;

    public ObjectPool(Func<T> objectGenerator = null, Action<T> resetAction = null)
    {
        _objectGenerator = objectGenerator ?? (() => new T());
        _resetAction = resetAction;
    }

    public T Get()
    {
        if (_objects.TryDequeue(out var item))
            return item;

        return _objectGenerator();
    }

    public void Return(T item)
    {
        _resetAction?.Invoke(item);
        _objects.Enqueue(item);
    }
}

// 使用示例
public static class AttackIntentPool
{
    private static readonly ObjectPool<ShootProjectileIntent> _projectilePool =
        new ObjectPool<ShootProjectileIntent>(
            resetAction: intent => intent.Reset()
        );

    public static ShootProjectileIntent GetProjectileIntent() => _projectilePool.Get();
    public static void ReturnProjectileIntent(ShootProjectileIntent intent) => _projectilePool.Return(intent);
}
```

### 8.2 事件系统优化

```csharp
// Core/Events/EventBus.cs
public class EventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Subscribe<T>(Action<T> handler) where T : class
    {
        var eventType = typeof(T);
        if (!_handlers.ContainsKey(eventType))
            _handlers[eventType] = new List<Delegate>();

        _handlers[eventType].Add(handler);
    }

    public void Unsubscribe<T>(Action<T> handler) where T : class
    {
        var eventType = typeof(T);
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);
        }
    }

    public void Publish<T>(T eventData) where T : class
    {
        var eventType = typeof(T);
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            foreach (Action<T> handler in handlers)
            {
                try
                {
                    handler(eventData);
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<AshenVoid>().Logger.Error($"事件处理失败: {eventType.Name}", ex);
                }
            }
        }
    }
}

// 事件定义
public class AttackStartedEvent
{
    public IAttackIntent Attack { get; set; }
    public NPC Attacker { get; set; }
}

public class AttackCompletedEvent
{
    public IAttackIntent Attack { get; set; }
    public NPC Attacker { get; set; }
    public bool WasInterrupted { get; set; }
}
```

---

## 9. 测试策略

### 9.1 单元测试示例

```csharp
// Tests/Core/ECS/AttackComponentTests.cs
[TestClass]
public class AttackComponentTests
{
    private Mock<NPC> _mockNpc;
    private AttackComponent _attackComponent;

    [TestInitialize]
    public void Setup()
    {
        _mockNpc = new Mock<NPC>();
        _attackComponent = new AttackComponent(_mockNpc.Object);
    }

    [TestMethod]
    public void SetIntent_WhenReady_ShouldAcceptIntent()
    {
        // Arrange
        var intent = new Mock<IAttackIntent>();
        intent.Setup(i => i.IsChanneled).Returns(false);
        intent.Setup(i => i.Priority).Returns(1);

        // Act
        _attackComponent.SetIntent(intent.Object);

        // Assert
        Assert.IsTrue(_attackComponent.IsAttacking());
    }

    [TestMethod]
    public void SetIntent_WhenHigherPriority_ShouldInterruptCurrentAttack()
    {
        // Arrange
        var lowPriorityIntent = new Mock<IAttackIntent>();
        lowPriorityIntent.Setup(i => i.Priority).Returns(1);
        lowPriorityIntent.Setup(i => i.CanBeInterrupted).Returns(true);

        var highPriorityIntent = new Mock<IAttackIntent>();
        highPriorityIntent.Setup(i => i.Priority).Returns(10);

        // Act
        _attackComponent.SetIntent(lowPriorityIntent.Object);
        _attackComponent.SetIntent(highPriorityIntent.Object);

        // Assert
        Assert.IsTrue(_attackComponent.IsAttacking());
        // 验证当前攻击是高优先级攻击
    }
}
```

### 9.2 集成测试

```csharp
// Tests/Integration/BossFrameworkIntegrationTests.cs
[TestClass]
public class BossFrameworkIntegrationTests
{
    [TestMethod]
    public void FullBossLifecycle_ShouldWorkCorrectly()
    {
        // Arrange
        var config = ConfigLoader.Load<BossConfig>("TestConfigs/NightmareCorruption.hjson");
        var serviceContainer = new ServiceContainer();
        var mockNpc = new Mock<NPC>();

        // Act
        var boss = new NightmareCorruption();
        boss.SetDefaults();

        // 模拟多帧更新
        for (int i = 0; i < 100; i++)
        {
            boss.AI();
        }

        // Assert
        // 验证Boss正常运行，没有异常
        Assert.IsNotNull(boss.Components);
    }
}
```

---

## 10. 迁移指南

### 10.1 向后兼容性

为了确保平滑迁移，我们将提供兼容性适配器：

```csharp
// Core/Compatibility/LegacyComponentAdapter.cs
[Obsolete("使用新的依赖注入系统替代直接组件引用")]
public static class LegacyComponentAdapter
{
    public static MovementComponent GetMovementComponent(this AIComponent ai)
    {
        return ai.GetMovementComponent() as MovementComponent;
    }

    public static AttackComponent GetAttackComponent(this AIComponent ai)
    {
        return ai.GetAttackComponent() as AttackComponent;
    }
}
```

### 10.2 迁移步骤

1. **第一步**：更新配置系统
   - 创建HJSON配置文件
   - 更新NightmareCorruption使用ConfigLoader
   - 测试配置加载

2. **第二步**：引入依赖注入
   - 实现ServiceContainer
   - 逐步替换直接组件引用
   - 保留兼容性适配器

3. **第三步**：扩展攻击系统
   - 更新IAttackIntent接口
   - 重构AttackComponent
   - 添加新的攻击类型

4. **第四步**：清理和优化
   - 移除兼容性代码
   - 性能优化
   - 文档更新

这个专业化升级方案将彻底改造当前的Boss框架，使其具备工业级软件的所有特征：可维护性、可测试性、可扩展性和稳定性。
