using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Terraria.GameContent.Animations.On_Actions;

namespace AshenVoid.Content.NPCs.CorruptedLord
{
    public class CorruptedLord : ModNPC
    {
        private int shadowTimer = 0;
        private enum AIState { Phase1, Phase2, Enraged }
        private enum Phase1State { Normal, Summoning, Charging }
        private enum Phase2State { Orbiting, Flying, Illusion }

        private AIState currentState = AIState.Phase1;
        private Phase1State phase1State = Phase1State.Normal;
        private Phase2State phase2State = Phase2State.Orbiting;

        // 状态变量
        private float phaseTransitionTimer = 0f;
        private float summonTimer = 0f;
        private float teleportTimer = 0f;
        private float shakeOffset = 0f;
        private float poisonTimer = 0f;
        private float orbitTimer = 0f;
        private int summonCount = 0;
        private float illusionTimer = 0f;
        private int orbitCount = 0;
        private const float orbitDistance = 400f; // 增加旋转距离
        private float rotationAngle = 0f; // 确保rotationAngle已声明

        // 其他变量
        private bool enraged = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1; // 只有一帧贴图
        }

        private float spawnTimer = 0f;
        private bool spawnAnimationComplete = false;

        public override void SetDefaults()
        {
            NPC.width = 85;
            NPC.height = 90;
            NPC.lifeMax = 13100;
            NPC.damage = 52;
            NPC.defense = 10;
            NPC.knockBackResist = 0f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 3, 0, 0);
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.netAlways = true;
            NPC.alpha = 255; // 初始完全透明
        }

        public override bool PreAI()
        {
            // 在AI执行前调用
            return base.PreAI(); // 返回true允许执行AI
        }

        public override void PostAI()
        {
            // 在AI执行后调用
            base.PostAI();
        }

        private void SpawnAnimation()
        {
            spawnTimer += 0.02f; // 控制动画速度

            // 渐显效果
            NPC.alpha = (int)(255 * (1 - spawnTimer));
            NPC.velocity.Y = 2f; // 缓慢下落

            // 粒子效果
            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustPerfect(NPC.Center + new Vector2(0, -50),
                    DustID.Corruption,
                    Main.rand.NextVector2Circular(3f, 3f));
                dust.scale = 1.8f;
                dust.noGravity = true;
            }

            // 传送门效果
            if (spawnTimer % 0.2f < 0.1f)
            {
                for (int i = 0; i < 2; i++)
                {
                    Dust dust = Dust.NewDustPerfect(NPC.Center + new Vector2(0, -80),
                        DustID.Shadowflame,
                        new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-0.5f, -2f)));
                    dust.scale = 2f;
                    dust.noGravity = true;
                }
            }

            // 动画完成
            if (spawnTimer >= 1f)
            {
                spawnAnimationComplete = true;
                NPC.alpha = 0;
                NPC.velocity = Vector2.Zero;

                // 震撼登场效果
                for (int i = 0; i < 30; i++)
                {
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                        DustID.Shadowflame, 0f, 0f, 100, default, 2f);
                    dust.velocity *= 3f;
                    dust.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.Item82, NPC.Center);

                // 发射圆环弹幕
                ShootRingProjectiles();
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!spawnAnimationComplete)
            {
                // 绘制传送门
                Texture2D portalTexture = ModContent.Request<Texture2D>("Terraria/Images/Extra_89").Value;
                float portalScale = 0.5f + spawnTimer * 0.5f;
                Color portalColor = Color.Purple * (0.5f * spawnTimer);

                spriteBatch.Draw(portalTexture,
                    NPC.Center + new Vector2(0, -80) - screenPos,
                    null,
                    portalColor,
                    0f,
                    portalTexture.Size() / 2f,
                    portalScale,
                    SpriteEffects.None,
                    0f);
            }

            return true;
        }

        private int healTimer = 0;
        public override void AI()
        {
            // 虚影回血机制(每10秒触发)
            if (NPC.life <= NPC.lifeMax * 0.1f) // 仅在10%血量以下触发
            {
                healTimer++;
                if (healTimer >= 600) // 10秒(60fps*10)
                {
                    healTimer = 0;
                    int shadowsToKill = Main.rand.Next(3, 11);
                    int shadowsKilled = 0;

                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<CorruptedLordPhantom>())
                        {
                            Main.npc[i].life = 0;
                            Main.npc[i].checkDead();
                            shadowsKilled++;

                            if (shadowsKilled >= shadowsToKill) break;
                        }
                    }

                    if (shadowsKilled > 0)
                    {
                        int healAmount = (int)(NPC.lifeMax * 0.15f * shadowsKilled / 3f);
                        NPC.life = Math.Min(NPC.life + healAmount, NPC.lifeMax);
                        CombatText.NewText(NPC.getRect(), Color.LimeGreen, $"恢复 +{healAmount}生命");
                    }
                }
            }
            base.AI(); // 调用基类AI逻辑

            // 设置弹幕不会伤害自己
            NPC.immune[NPC.whoAmI] = 50;

            if (!spawnAnimationComplete)
            {
                // 初始生成位置设置为玩家头顶5-10格随机高度(160-320像素)
                if (spawnTimer == 0f)
                {
                    float spawnHeight = Main.rand.Next(160, 321); // 5-10格随机高度
                    NPC.position = Main.player[NPC.target].Center - new Vector2(NPC.width / 2, NPC.height / 2 + spawnHeight);
                }

                SpawnAnimation();
                return;
            }

            Player player = Main.player[NPC.target];

            // 移动轨迹粒子效果
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Shadowflame, 0f, 0f, 100, default, 1.5f);
                dust.velocity *= 0.3f;
                dust.noGravity = true;
            }

            // 确保只在服务器端执行AI
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                {
                    NPC.TargetClosest();
                }
            }

            CheckEnrageConditions(player);

            switch (currentState)
            {
                case AIState.Phase1:
                    Phase1AI(player);
                    break;

                case AIState.Phase2:
                    Phase2AI(player);
                    break;

                case AIState.Enraged:
                    if (NPC.life > NPC.lifeMax * 0.5f)
                        Phase1AI(player, enraged: true);
                    else
                        Phase2AI(player, enraged: true);
                    break;
            }

            phaseTransitionTimer++;
            summonTimer++;
            teleportTimer++;
        }

        private void CheckEnrageConditions(Player player)
        {
            bool inEvilBiome = player.ZoneCorrupt || player.ZoneCrimson;
            bool onSurface = player.position.Y < Main.worldSurface * 16.0;

            if (!inEvilBiome || onSurface)
            {
                currentState = AIState.Enraged;
                // 狂暴视觉效果
                if (Main.rand.NextBool(10))
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Shadowflame, 0f, 0f, 100, default, 2f);
                }
            }
            else if (NPC.life <= NPC.lifeMax * 0.5f)
            {
                currentState = AIState.Phase2;
                // 阶段转换效果
                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Corruption, 0f, 0f, 100, default, 1.5f);
                }
                SoundEngine.PlaySound(SoundID.Item105, NPC.Center);
            }
            else
            {
                currentState = AIState.Phase1;
            }
        }

        private void Phase1AI(Player player, bool enraged = false)
        {
            // 固定在屏幕上方(降低高度)
            Vector2 targetPos = player.Center - new Vector2(0, Main.screenHeight * 0.3f);
            NPC.position = Vector2.Lerp(NPC.position, targetPos, 0.05f);

            // 贴图抖动效果
            shakeOffset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10f) * 2f;
            NPC.position.Y += shakeOffset;

            // 增强粒子效果
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Shadowflame, 0f, 0f, 150, default, 2f);
                dust.velocity *= 0.4f;
                dust.noGravity = true;

                Dust shadowDust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Corruption, 0f, 0f, 150, default, 1.8f);
                shadowDust.velocity *= 0.3f;
                shadowDust.noGravity = true;
            }

            // 状态机逻辑
            switch (phase1State)
            {
                case Phase1State.Normal:
                    // 周期性发射毒液弹幕
                    poisonTimer++;
                    if (poisonTimer >= 90f) // 每1.5秒发射一次
                    {
                        ShootPoison(player);
                        poisonTimer = 0f;
                    }

                    // 召唤3次小弟
                    summonTimer++;
                    float summonInterval = enraged ? 210f : 420f; // 狂暴时3.5秒，普通7秒
                    if (summonTimer >= summonInterval)
                    {
                        for (int i = 0; i < 2; i++) // 一次召唤2只
                        {
                            SummonMinions();
                        }
                        summonCount++;
                        summonTimer = 0f;

                        if (summonCount >= 3)
                        {
                            phase1State = Phase1State.Charging;
                            summonCount = 0;
                            // 蓄力特效
                        }
                    }
                    break;

                case Phase1State.Charging:
                    // 4秒蓄力
                    summonTimer++;
                    if (summonTimer >= 240f) // 4秒
                    {
                        // 召唤4个腐化地怪物
                        for (int i = 0; i < 4; i++)
                        {
                            SummonCorruptedGroundMonster();
                        }
                        summonTimer = 0f;
                        phase1State = Phase1State.Normal;
                    }
                    break;
            }
        }

        private void ShootPoison(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // 在玩家周围随机位置生成毒雾
            for (int i = 0; i < 3; i++) // 生成3团毒雾
            {
                Vector2 spawnPos = player.Center + new Vector2(
                    Main.rand.Next(-200, 200),
                    Main.rand.Next(-100, 100));

                // 使用自定义毒雾弹幕或原版毒云
                int type = ProjectileID.CorruptSpray; // 腐化者的毒云弹幕                
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.immune[NPC.whoAmI] = 10;
                }
                Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, Vector2.Zero,
                    type, NPC.damage / 3, 0f, Main.myPlayer);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.immune[NPC.whoAmI] = 0;
                }
            }

            // 发射特效
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.ToxicBubble, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
            }
            SoundEngine.PlaySound(SoundID.Item17, NPC.Center);
        }

        private void SummonCorruptedGroundMonster()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int type = NPCID.Corruptor; // 腐化者
            Vector2 spawnPos = NPC.Center + new Vector2(Main.rand.Next(-200, 200), Main.rand.Next(-200, 200));

            int minion = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y, type);

            if (Main.netMode == NetmodeID.Server && minion < Main.maxNPCs)
            {
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, minion);
            }

            // 召唤特效
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDustDirect(spawnPos, 0, 0, DustID.Corruption, 0f, 0f, 100, default, 1.5f);
            }
            SoundEngine.PlaySound(SoundID.NPCDeath6, spawnPos);
        }

        private void Phase2AI(Player player, bool enraged = false)
        {
            // 分阶段召唤虚影(添加调试输出)
            if (NPC.life <= NPC.lifeMax * 0.2f && !NPC.dontTakeDamage)
            {
                int summonInterval = 180; // 默认3秒
                if (NPC.life <= NPC.lifeMax * 0.05f)
                {
                    summonInterval = 12; // 0.2秒
                    if (shadowTimer % 60 == 0)
                        Main.NewText("极限阶段：虚影加速召唤！", Color.Red);
                }
                else if (shadowTimer == 0)
                {
                    Main.NewText("虚影召唤阶段启动", Color.Purple);
                }

                if (shadowTimer <= 0)
                {
                    Vector2 playerCenter = Main.player[NPC.target].Center;
                    float distance = Main.rand.Next(160, 961); // 5-30格随机距离(32*5=160, 32*30=960)
                    Vector2 spawnPos = playerCenter + new Vector2(0, distance).RotatedByRandom(MathHelper.TwoPi);

                    int shadow = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y,
                        ModContent.NPCType<CorruptedLordPhantom>());

                    if (shadow < Main.maxNPCs)
                    {
                        Main.npc[shadow].ai[0] = 1; // 标记为永久虚影
                        Main.npc[shadow].ai[1] = Main.rand.NextFloat(MathHelper.TwoPi); // 随机起始角度
                        Main.npc[shadow].ai[2] = distance; // 存储距离
                        Main.npc[shadow].life = 9999; // 高血量
                        Main.npc[shadow].dontTakeDamage = true; // 无敌
                        Main.npc[shadow].damage = NPC.damage / 2;
                        Main.npc[shadow].alpha = 130;
                        Main.npc[shadow].netUpdate = true;
                    }

                    shadowTimer = summonInterval;
                }
                shadowTimer--;
            }

            // 处理永久虚影的旋转行为
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<CorruptedLordPhantom>() && Main.npc[i].ai[0] == 1)
                {
                    // 围绕玩家旋转
                    float rotationSpeed = 0.03f;
                    Main.npc[i].ai[1] += rotationSpeed;

                    Vector2 playerCenter = Main.player[NPC.target].Center;
                    float distance = Main.npc[i].ai[2];
                    Vector2 targetPos = playerCenter + new Vector2(
                        (float)Math.Cos(Main.npc[i].ai[1]) * distance,
                        (float)Math.Sin(Main.npc[i].ai[1]) * distance * 0.6f);

                    Main.npc[i].Center = Vector2.Lerp(Main.npc[i].Center, targetPos, 0.1f);

                    // 每2秒发射一次腐化弹幕
                    if (Main.npc[i].localAI[0] % 120 == 0)
                    {
                        Vector2 velocity = (playerCenter - Main.npc[i].Center).SafeNormalize(Vector2.Zero) * 6f;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.immune[NPC.whoAmI] = 10;
                        }
                        Projectile.NewProjectile(
                            Main.npc[i].GetSource_FromAI(),
                            Main.npc[i].Center,
                            velocity,
                            ProjectileID.CursedFlare, // 改为诅咒火焰弹幕
                            Main.npc[i].damage,
                            0f,
                            Main.myPlayer);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.immune[NPC.whoAmI] = 0;
                        }
                    }
                    Main.npc[i].localAI[0]++;
                }
            }

            switch (phase2State)
            {
                case Phase2State.Orbiting:
                    // 环绕玩家1圈，速度提升50%
                    orbitTimer++;
                    float orbitSpeed = enraged ? 0.015f : 0.01f; // 速度提升50%
                    float orbitRadius = enraged ? 350f : 450f;

                    rotationAngle += orbitSpeed;
                    if (rotationAngle >= MathHelper.TwoPi) // 1圈
                    {
                        rotationAngle = 0f;
                        phase2State = Phase2State.Flying;
                        orbitTimer = 0f;
                    }

                    Vector2 orbitPos = player.Center + new Vector2(
                        (float)Math.Cos(rotationAngle) * orbitDistance,
                        (float)Math.Sin(rotationAngle) * orbitDistance * 0.6f);

                    NPC.position = Vector2.Lerp(NPC.position, orbitPos, 0.05f);
                    NPC.rotation = 0f; // 禁用贴图自转

                    // 环绕特效
                    if (Main.rand.NextBool(3))
                    {
                        Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                            DustID.Shadowflame, 0f, 0f, 150, default, 1.8f);
                    }

                    // 周期性发射毒液弹幕
                    poisonTimer++;
                    float poisonInterval = enraged ? 45f : 60f; // 狂暴时发射更快
                    if (poisonTimer >= poisonInterval)
                    {
                        ShootPoison(player);
                        poisonTimer = 0f;
                    }
                    break;

                case Phase2State.Flying:
                    // 飞到固定高度
                    Vector2 flyPos = player.Center - new Vector2(0, Main.screenHeight * 0.4f);
                    NPC.position = Vector2.Lerp(NPC.position, flyPos, 0.03f);

                    orbitTimer++;
                    float flyDuration = enraged ? 90f : 120f; // 狂暴时更快进入幻影阶段
                    if (orbitTimer >= flyDuration)
                    {
                        phase2State = Phase2State.Illusion;
                        CreateIllusions();
                        orbitTimer = 0f;
                    }
                    break;

                case Phase2State.Illusion:
                    // 幻影阶段
                    illusionTimer++;
                    float illusionDuration = enraged ? 240f : 300f; // 狂暴时幻影持续时间更短
                    if (illusionTimer >= illusionDuration)
                    {
                        phase2State = Phase2State.Orbiting;
                        illusionTimer = 0f;
                        RemoveIllusions();
                    }

                    // 本体抖动效果
                    if (Main.rand.NextBool(5))
                    {
                        NPC.position += Main.rand.NextVector2Circular(4f, 4f);
                    }

                    // 狂暴时额外召唤
                    if (enraged && summonTimer >= 180f)
                    {
                        SummonMinions();
                        summonTimer = 0f;
                    }
                    break;
            }

            summonTimer++;
        }

        private void SummonMinions()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // 召唤腐化类敌怪
            int[] minionTypes = new int[] { NPCID.EaterofSouls, NPCID.DevourerHead, NPCID.Corruptor };
            int type = minionTypes[Main.rand.Next(minionTypes.Length)];

            // 在Boss周围随机位置召唤
            Vector2 spawnPos = NPC.Center + new Vector2(Main.rand.Next(-100, 100), Main.rand.Next(-100, 100));
            int minion = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y, type);

            // 确保敌怪生成在服务器端
            if (Main.netMode == NetmodeID.Server && minion < Main.maxNPCs)
            {
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, minion);
            }

            // 召唤粒子效果
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(spawnPos, NPC.width, NPC.height, DustID.CursedTorch);
                dust.velocity = Main.rand.NextVector2Circular(5f, 5f);
                dust.scale = 1.5f;
            }

            // 召唤音效
            SoundEngine.PlaySound(SoundID.Item82, NPC.Center);

            // 调试日志
            if (Main.netMode == NetmodeID.Server)
            {
                Mod.Logger.Debug($"Summoned minion {type} at {spawnPos}");
            }
        }

        private void ShootRingProjectiles()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int type = ProjectileID.CursedDart; // 使用诅咒飞镖弹幕
            int damage = NPC.damage / 3;
            float speed = 6f;
            int numberProjectiles = 16; // 16方向弹幕环

            for (int i = 0; i < numberProjectiles; i++)
            {
                Vector2 velocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / numberProjectiles) * speed;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                    type, damage, 0f, Main.myPlayer);
            }

            // 弹幕发射特效
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.PurpleTorch, 0f, 0f, 150, default, 2f);
            }
            SoundEngine.PlaySound(SoundID.Item117, NPC.Center);
        }

        private void CreateIllusions()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // 创建2个虚影固定在玩家左右上角
            for (int i = 0; i < 2; i++)
            {
                // 计算固定位置：左右上角
                Vector2 spawnPos = Main.player[Main.myPlayer].Center + new Vector2(i == 0 ? -300 : 300, -100);


                int illusion = NPC.NewNPC(
                    NPC.GetSource_FromAI(),
                    (int)spawnPos.X,
                    (int)spawnPos.Y,
                    NPC.type);

                if (illusion < Main.maxNPCs)
                {
                    Main.npc[illusion].ai[0] = 1; // 标记为虚影
                    Main.npc[illusion].ai[1] = i; // 存储虚影索引(0=左,1=右)
                    Main.npc[illusion].ai[2] = 600; // 10秒计时器(60fps*10=600)
                    Main.npc[illusion].life = 1;
                    Main.npc[illusion].dontTakeDamage = true;
                    Main.npc[illusion].damage = NPC.damage / 2; // 可以造成伤害
                    Main.npc[illusion].alpha = 130;
                    Main.npc[illusion].velocity = Vector2.Zero; // 固定不动
                    Main.npc[illusion].netUpdate = true;

                    // 虚影生成特效
                    for (int j = 0; j < 15; j++)
                    {
                        Dust.NewDustDirect(spawnPos, NPC.width, NPC.height,
                            DustID.Shadowflame, 0f, 0f, 150, default, 2f);
                    }
                }
            }

            // 本体特效
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.PurpleTorch, 0f, 0f, 150, default, 2.5f);
            }
            SoundEngine.PlaySound(SoundID.Item117, NPC.Center);
        }

        private void RemoveIllusions()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == NPC.type && Main.npc[i].ai[0] == 1)
                {
                    // 幻影消失特效
                    for (int j = 0; j < 10; j++)
                    {
                        Dust.NewDustDirect(Main.npc[i].position, Main.npc[i].width, Main.npc[i].height,
                            DustID.Shadowflame, 0f, 0f, 150, default, 2f);
                    }

                    Main.npc[i].active = false;
                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, i);
                    }
                }
            }

            // 本体特效
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.PurpleTorch, 0f, 0f, 150, default, 2f);
            }
            SoundEngine.PlaySound(SoundID.Item104, NPC.Center);
        }

        public override bool CheckDead()
        {
            // 清除所有虚影
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active &&
                   (Main.npc[i].type == ModContent.NPCType<CorruptedLordPhantom>() ||
                   Main.npc[i].type == NPC.type && Main.npc[i].ai[0] == 1))
                {
                    Main.npc[i].active = false;
                    Main.npc[i].netUpdate = true;
                }
            }

            // 死亡特效
            for (int i = 0; i < 50; i++)
            {
                Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Shadowflame, 0f, 0f, 100, default, 2f);
            }
            SoundEngine.PlaySound(SoundID.NPCDeath55, NPC.Center);

            return true;
        }

        private void TeleportAttack(Player player)
        {
            // 在玩家周围随机位置传送
            Vector2 teleportPos = player.Center + new Vector2(Main.rand.Next(-600, 600), Main.rand.Next(-400, 400));
            NPC.position = teleportPos;
            NPC.velocity *= 0.5f; // 传送后速度减半

            // 传送效果
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDustPerfect(NPC.Center, DustID.Shadowflame,
                    Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.5f);
            }
            SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
        }

    }
}