using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 游戏调试控制台管理器
/// 提供游戏运行时、UI、网络、场景等调试功能
/// </summary>
public class ConsoleManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

/// <summary>
/// 调试控制台编辑器窗口
/// </summary>
public class DebugConsoleWindow : EditorWindow
{
    #region 窗口管理
    private Vector2 scrollPosition;
    private int selectedTab = 0;
    private readonly string[] tabNames = { "游戏控制", "UI调试", "网络调试", "场景管理", "性能监控", "道具系统" };
    #endregion

    #region 游戏控制相关
    private bool isGamePaused = false;
    private float timeScale = 1.0f;
    private bool godMode = false;
    private bool infiniteMana = false;
    private bool noCooldown = false;
    #endregion

    #region UI调试相关
    private bool showFPS = false;
    private bool showDebugInfo = false;
    private bool uiDebugMode = false;
    private bool showHitBoxes = false;
    #endregion

    #region 网络调试相关
    private string serverIP = "127.0.0.1";
    private int serverPort = 8080;
    private bool isConnected = false;
    private bool autoReconnect = true;
    #endregion

    #region 场景管理相关
    private string[] sceneNames = { "MainMenu", "GameWorld", "BattleScene", "ShopScene", "InventoryScene" };
    private int selectedSceneIndex = 0;
    #endregion

    #region 性能监控相关
    private bool enableProfiler = false;
    private bool showMemoryUsage = false;
    private bool logPerformance = false;
    #endregion

    #region 道具系统相关
    private int goldAmount = 1000;
    private int expAmount = 100;
    private int level = 1;
    private bool unlockAllItems = false;
    #endregion

    [MenuItem("Debug/游戏控制台")]
    public static void ShowWindow()
    {
        DebugConsoleWindow window = GetWindow<DebugConsoleWindow>("游戏控制台");
        window.minSize = new Vector2(400, 600);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        
        // 标题
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🎮 游戏调试控制台", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        // 标签页
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
        EditorGUILayout.Space(10);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // 根据选中的标签页显示不同内容
        switch (selectedTab)
        {
            case 0:
                DrawGameControlTab();
                break;
            case 1:
                DrawUIDebugTab();
                break;
            case 2:
                DrawNetworkDebugTab();
                break;
            case 3:
                DrawSceneManagementTab();
                break;
            case 4:
                DrawPerformanceTab();
                break;
            case 5:
                DrawItemSystemTab();
                break;
        }
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    #region 游戏控制标签页
    private void DrawGameControlTab()
    {
        EditorGUILayout.LabelField("🎯 游戏控制", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // 时间控制
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("⏰ 时间控制", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(isGamePaused ? "▶️ 继续游戏" : "⏸️ 暂停游戏"))
        {
            isGamePaused = !isGamePaused;
            Time.timeScale = isGamePaused ? 0f : timeScale;
        }
        
        if (GUILayout.Button("🔄 重置时间"))
        {
            timeScale = 1.0f;
            Time.timeScale = isGamePaused ? 0f : timeScale;
        }
        EditorGUILayout.EndHorizontal();
        
        timeScale = EditorGUILayout.Slider("时间倍率", timeScale, 0.1f, 5.0f);
        if (!isGamePaused)
        {
            Time.timeScale = timeScale;
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 角色状态
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("👤 角色状态", EditorStyles.boldLabel);
        
        godMode = EditorGUILayout.Toggle("🛡️ 无敌模式", godMode);
        infiniteMana = EditorGUILayout.Toggle("💫 无限法力", infiniteMana);
        noCooldown = EditorGUILayout.Toggle("⚡ 无冷却", noCooldown);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("💖 满血"))
        {
            Debug.Log("执行满血操作");
        }
        if (GUILayout.Button("💀 死亡"))
        {
            Debug.Log("执行死亡操作");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 快速操作
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("⚡ 快速操作", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🏃 传送到安全点"))
        {
            Debug.Log("传送到安全点");
        }
        if (GUILayout.Button("🎯 传送到Boss"))
        {
            Debug.Log("传送到Boss");
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("💎 获得所有技能"))
        {
            Debug.Log("获得所有技能");
        }
        if (GUILayout.Button("🗡️ 获得所有装备"))
        {
            Debug.Log("获得所有装备");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    #endregion

    #region UI调试标签页
    private void DrawUIDebugTab()
    {
        EditorGUILayout.LabelField("🖥️ UI调试", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // 显示控制
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("📊 显示控制", EditorStyles.boldLabel);
        
        showFPS = EditorGUILayout.Toggle("📈 显示FPS", showFPS);
        showDebugInfo = EditorGUILayout.Toggle("🔍 显示调试信息", showDebugInfo);
        showHitBoxes = EditorGUILayout.Toggle("📦 显示碰撞框", showHitBoxes);
        uiDebugMode = EditorGUILayout.Toggle("🎨 UI调试模式", uiDebugMode);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // UI操作
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🎮 UI操作", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("📋 打开背包"))
        {
            Debug.Log("打开背包界面");
        }
        if (GUILayout.Button("⚔️ 打开技能"))
        {
            Debug.Log("打开技能界面");
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🏪 打开商店"))
        {
            Debug.Log("打开商店界面");
        }
        if (GUILayout.Button("📜 打开任务"))
        {
            Debug.Log("打开任务界面");
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("⚙️ 打开设置"))
        {
            Debug.Log("打开设置界面");
        }
        if (GUILayout.Button("❌ 关闭所有UI"))
        {
            Debug.Log("关闭所有UI");
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // UI测试
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🧪 UI测试", EditorStyles.boldLabel);
        
        if (GUILayout.Button("💬 显示测试弹窗"))
        {
            Debug.Log("显示测试弹窗");
        }
        
        if (GUILayout.Button("📱 切换分辨率"))
        {
            Debug.Log("切换分辨率");
        }
        
        if (GUILayout.Button("🌙 切换主题"))
        {
            Debug.Log("切换主题");
        }
        EditorGUILayout.EndVertical();
    }
    #endregion

    #region 网络调试标签页
    private void DrawNetworkDebugTab()
    {
        EditorGUILayout.LabelField("🌐 网络调试", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // 连接设置
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🔗 连接设置", EditorStyles.boldLabel);
        
        serverIP = EditorGUILayout.TextField("服务器IP", serverIP);
        serverPort = EditorGUILayout.IntField("端口", serverPort);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(isConnected ? "🔌 断开连接" : "🔗 连接服务器"))
        {
            isConnected = !isConnected;
            Debug.Log(isConnected ? "连接到服务器" : "断开服务器连接");
        }
        if (GUILayout.Button("🔄 重连"))
        {
            Debug.Log("重新连接服务器");
        }
        EditorGUILayout.EndHorizontal();
        
        autoReconnect = EditorGUILayout.Toggle("🔄 自动重连", autoReconnect);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 网络状态
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("📊 网络状态", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField($"连接状态: {(isConnected ? "已连接" : "未连接")}");
        EditorGUILayout.LabelField($"延迟: {Random.Range(10, 100)}ms");
        EditorGUILayout.LabelField($"上传速度: {Random.Range(100, 1000)}KB/s");
        EditorGUILayout.LabelField($"下载速度: {Random.Range(500, 2000)}KB/s");
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 网络测试
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🧪 网络测试", EditorStyles.boldLabel);
        
        if (GUILayout.Button("📤 发送测试数据"))
        {
            Debug.Log("发送测试数据包");
        }
        
        if (GUILayout.Button("📥 模拟网络延迟"))
        {
            Debug.Log("模拟网络延迟");
        }
        
        if (GUILayout.Button("💥 模拟网络断开"))
        {
            Debug.Log("模拟网络断开");
        }
        EditorGUILayout.EndVertical();
    }
    #endregion

    #region 场景管理标签页
    private void DrawSceneManagementTab()
    {
        EditorGUILayout.LabelField("🌍 场景管理", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // 场景跳转
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🚀 场景跳转", EditorStyles.boldLabel);
        
        selectedSceneIndex = EditorGUILayout.Popup("选择场景", selectedSceneIndex, sceneNames);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🎬 跳转场景"))
        {
            Debug.Log($"跳转到场景: {sceneNames[selectedSceneIndex]}");
        }
        if (GUILayout.Button("🔄 重新加载当前场景"))
        {
            Debug.Log("重新加载当前场景");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 场景信息
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("📋 场景信息", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField($"当前场景: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        EditorGUILayout.LabelField($"场景对象数量: {FindObjectsOfType<GameObject>().Length}");
        EditorGUILayout.LabelField($"内存使用: {System.GC.GetTotalMemory(false) / 1024 / 1024}MB");
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 场景操作
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("⚙️ 场景操作", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🗑️ 清理场景垃圾"))
        {
            Debug.Log("清理场景垃圾");
        }
        
        if (GUILayout.Button("💾 保存场景"))
        {
            Debug.Log("保存场景");
        }
        
        if (GUILayout.Button("📊 场景性能分析"))
        {
            Debug.Log("开始场景性能分析");
        }
        EditorGUILayout.EndVertical();
    }
    #endregion

    #region 性能监控标签页
    private void DrawPerformanceTab()
    {
        EditorGUILayout.LabelField("📊 性能监控", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // 监控设置
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("⚙️ 监控设置", EditorStyles.boldLabel);
        
        enableProfiler = EditorGUILayout.Toggle("📈 启用性能分析器", enableProfiler);
        showMemoryUsage = EditorGUILayout.Toggle("💾 显示内存使用", showMemoryUsage);
        logPerformance = EditorGUILayout.Toggle("📝 记录性能日志", logPerformance);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 性能数据
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("📊 实时数据", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField($"FPS: {1.0f / Time.deltaTime:F1}");
        EditorGUILayout.LabelField($"帧时间: {Time.deltaTime * 1000:F2}ms");
        EditorGUILayout.LabelField($"内存使用: {System.GC.GetTotalMemory(false) / 1024 / 1024}MB");
        EditorGUILayout.LabelField($"DrawCall: {Random.Range(50, 200)}");
        EditorGUILayout.LabelField($"三角面数: {Random.Range(1000, 10000)}");
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 性能操作
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🔧 性能操作", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🗑️ 强制垃圾回收"))
        {
            System.GC.Collect();
            Debug.Log("执行垃圾回收");
        }
        
        if (GUILayout.Button("📊 生成性能报告"))
        {
            Debug.Log("生成性能报告");
        }
        
        if (GUILayout.Button("⚡ 优化场景"))
        {
            Debug.Log("执行场景优化");
        }
        EditorGUILayout.EndVertical();
    }
    #endregion

    #region 道具系统标签页
    private void DrawItemSystemTab()
    {
        EditorGUILayout.LabelField("🎒 道具系统", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // 角色属性
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("👤 角色属性", EditorStyles.boldLabel);
        
        goldAmount = EditorGUILayout.IntField("💰 金币数量", goldAmount);
        expAmount = EditorGUILayout.IntField("⭐ 经验值", expAmount);
        level = EditorGUILayout.IntField("📈 等级", level);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("💰 增加金币"))
        {
            goldAmount += 1000;
            Debug.Log($"增加1000金币，当前: {goldAmount}");
        }
        if (GUILayout.Button("⭐ 增加经验"))
        {
            expAmount += 100;
            Debug.Log($"增加100经验，当前: {expAmount}");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 道具操作
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🎁 道具操作", EditorStyles.boldLabel);
        
        unlockAllItems = EditorGUILayout.Toggle("🔓 解锁所有道具", unlockAllItems);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("⚔️ 获得武器"))
        {
            Debug.Log("获得随机武器");
        }
        if (GUILayout.Button("🛡️ 获得防具"))
        {
            Debug.Log("获得随机防具");
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("💊 获得药水"))
        {
            Debug.Log("获得治疗药水");
        }
        if (GUILayout.Button("📜 获得卷轴"))
        {
            Debug.Log("获得魔法卷轴");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 快速设置
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("⚡ 快速设置", EditorStyles.boldLabel);
        
        if (GUILayout.Button("💎 获得所有稀有道具"))
        {
            Debug.Log("获得所有稀有道具");
        }
        
        if (GUILayout.Button("🗑️ 清空背包"))
        {
            Debug.Log("清空背包");
        }
        
        if (GUILayout.Button("🔄 重置角色数据"))
        {
            goldAmount = 1000;
            expAmount = 100;
            level = 1;
            Debug.Log("重置角色数据");
        }
        EditorGUILayout.EndVertical();
    }
    #endregion
}
