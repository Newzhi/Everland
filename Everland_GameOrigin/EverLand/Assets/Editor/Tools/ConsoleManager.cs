using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 游戏调试控制台管理器
/// 提供游戏运行时、UI、网络、场景等调试功能
/// 注意：这是一个编辑器工具，不需要继承MonoBehaviour
/// </summary>
public class ConsoleManager
{
    // 静态工具类，提供调试功能的接口
    public static void LogDebugInfo(string message)
    {
        Debug.Log($"[调试控制台] {message}");
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
    private readonly string[] tabNames = { "游戏内测试", "网络调试", "场景管理", "性能监控" };
    
    // 游戏内测试子标签页管理
    private int gameTestSubTab = 0;
    private readonly string[] gameTestSubTabNames = { "游戏控制", "道具系统", "UI测试" };
    
    // 可扩展配置：添加新的子标签页时，只需修改以下数组
    // 注意：添加新标签页时，需要在DrawGameTestTab()的switch语句中添加对应的case
    // 
    // 扩展示例：
    // 1. 修改数组：private readonly string[] gameTestSubTabNames = { "游戏控制", "道具系统", "UI测试", "战斗测试" };
    // 2. 添加方法：private void DrawCombatTestSubTab() { /* 战斗测试UI */ }
    // 3. 添加case：case 3: DrawCombatTestSubTab(); break;
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
    private float networkLatency = 0f;
    private float uploadSpeed = 0f;
    private float downloadSpeed = 0f;
    #endregion

    #region 场景管理相关
    private string[] sceneNames = { "SampleScene", "TestScene" };
    private int selectedSceneIndex = 0;
    #endregion

    #region 性能监控相关
    private bool showMemoryUsage = false;
    private bool logPerformance = false;
    private float currentFPS = 0f;
    private float currentFrameTime = 0f;
    private long currentMemoryUsage = 0;
    private int currentDrawCalls = 0;
    private int currentTriangles = 0;
    
    // FPS平滑计算相关
    private float fpsUpdateInterval = 0.5f; // FPS更新间隔
    private float fpsAccumulator = 0f;
    private int fpsFrames = 0;
    private float fpsTimeLeft = 0f;
    private float smoothedFPS = 0f;
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

    private void Update()
    {
        // 只在游戏运行时更新性能数据
        if (Application.isPlaying)
        {
            UpdatePerformanceData();
        }
        else
        {
            // 游戏未运行时重置数据
            ResetPerformanceData();
        }
        
        // 强制重绘窗口以更新显示
        Repaint();
    }

    private void UpdatePerformanceData()
    {
        // 获取真实的帧时间（毫秒）
        currentFrameTime = Time.deltaTime * 1000f;
        
        // 使用平滑算法计算FPS，避免帧率波动
        fpsTimeLeft -= Time.deltaTime;
        fpsAccumulator += Time.timeScale / Time.deltaTime;
        fpsFrames++;
        
        if (fpsTimeLeft <= 0.0f)
        {
            smoothedFPS = fpsAccumulator / fpsFrames;
            currentFPS = smoothedFPS;
            fpsTimeLeft = fpsUpdateInterval;
            fpsAccumulator = 0.0f;
            fpsFrames = 0;
        }
        
        // 获取真实的内存使用情况
        currentMemoryUsage = System.GC.GetTotalMemory(false) / 1024 / 1024;
        
        // 获取真实的渲染统计信息
        currentDrawCalls = GetRealDrawCalls();
        currentTriangles = GetRealTriangles();
        
        // 更新网络状态（如果连接中）
        if (isConnected)
        {
            UpdateNetworkStats();
        }
        else
        {
            networkLatency = 0f;
            uploadSpeed = 0f;
            downloadSpeed = 0f;
        }
    }

    private void UpdateNetworkStats()
    {
        // 这里可以集成真实的网络统计API
        // 目前使用模拟数据，但可以根据实际网络库进行替换
        
        // 模拟网络延迟测试
        if (Time.time % 2f < 0.1f) // 每2秒更新一次
        {
            // 这里可以添加真实的ping测试
            networkLatency = Random.Range(10f, 100f);
        }
        
        // 模拟网络速度测试
        if (Time.time % 3f < 0.1f) // 每3秒更新一次
        {
            // 这里可以添加真实的网络速度测试
            uploadSpeed = Random.Range(100f, 1000f);
            downloadSpeed = Random.Range(500f, 2000f);
        }
    }

    private int GetRealDrawCalls()
    {
        // 尝试使用Unity Profiler API获取真实的DrawCall数量
        try
        {
            // 使用反射获取Unity内部统计信息
            var profilerType = System.Type.GetType("UnityEngine.Profiling.Profiler");
            if (profilerType != null)
            {
                var getDrawCallsMethod = profilerType.GetMethod("GetDrawCalls", 
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (getDrawCallsMethod != null)
                {
                    return (int)getDrawCallsMethod.Invoke(null, null);
                }
            }
        }
        catch
        {
            // 如果无法获取真实数据，使用备用方案
        }
        
        // 备用方案：通过统计场景中的Renderer组件数量来估算
        // 这是一个相对准确的估算方法
        var renderers = FindObjectsOfType<Renderer>();
        int drawCallCount = 0;
        
        foreach (var renderer in renderers)
        {
            // 每个Renderer通常对应一个DrawCall
            drawCallCount++;
            
            // 如果Renderer有多个材质，会增加额外的DrawCall
            if (renderer.materials != null)
            {
                drawCallCount += renderer.materials.Length - 1;
            }
        }
        
        return drawCallCount;
    }

    private int GetRealTriangles()
    {
        // 尝试使用Unity Profiler API获取真实的三角面数量
        try
        {
            var profilerType = System.Type.GetType("UnityEngine.Profiling.Profiler");
            if (profilerType != null)
            {
                var getTrianglesMethod = profilerType.GetMethod("GetTriangles", 
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (getTrianglesMethod != null)
                {
                    return (int)getTrianglesMethod.Invoke(null, null);
                }
            }
        }
        catch
        {
            // 如果无法获取真实数据，使用备用方案
        }
        
        // 备用方案：通过统计场景中的所有Mesh组件来精确计算
        int totalTriangles = 0;
        
        // 统计MeshRenderer的三角面
        var meshRenderers = FindObjectsOfType<MeshRenderer>();
        foreach (var renderer in meshRenderers)
        {
            var meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                totalTriangles += meshFilter.sharedMesh.triangles.Length / 3;
            }
        }
        
        // 统计SkinnedMeshRenderer的三角面
        var skinnedMeshRenderers = FindObjectsOfType<SkinnedMeshRenderer>();
        foreach (var renderer in skinnedMeshRenderers)
        {
            if (renderer.sharedMesh != null)
            {
                totalTriangles += renderer.sharedMesh.triangles.Length / 3;
            }
        }
        
        // 统计Terrain的三角面（估算）
        var terrains = FindObjectsOfType<Terrain>();
        foreach (var terrain in terrains)
        {
            if (terrain.terrainData != null)
            {
                // Terrain的三角面数 = (heightmap分辨率-1)² * 2
                int resolution = terrain.terrainData.heightmapResolution;
                totalTriangles += (resolution - 1) * (resolution - 1) * 2;
            }
        }
        
        return totalTriangles;
    }

    private int GetSceneObjectCount()
    {
        // 获取场景中真实的对象数量
        try
        {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var rootObjects = activeScene.GetRootGameObjects();
            int totalCount = 0;
            
            foreach (var rootObject in rootObjects)
            {
                totalCount += CountAllChildren(rootObject);
            }
            
            return totalCount;
        }
        catch
        {
            // 如果出错，使用备用方法
            return FindObjectsOfType<GameObject>().Length;
        }
    }

    private int CountAllChildren(GameObject obj)
    {
        int count = 1; // 计算自己
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            count += CountAllChildren(obj.transform.GetChild(i).gameObject);
        }
        return count;
    }

    private void ResetPerformanceData()
    {
        currentFPS = 0f;
        currentFrameTime = 0f;
        currentMemoryUsage = 0;
        currentDrawCalls = 0;
        currentTriangles = 0;
    }

    private void OnGUI()
    {
        try
        {
            EditorGUILayout.BeginVertical();
            
            // 标题和游戏状态
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🎮 游戏调试控制台", EditorStyles.boldLabel);
            
            // 显示游戏运行状态
            string gameStatus = Application.isPlaying ? "🟢 游戏运行中" : "🔴 游戏未运行";
            EditorGUILayout.LabelField($"状态: {gameStatus}", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);
            
            // 标签页
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            EditorGUILayout.Space(10);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // 根据选中的标签页显示不同内容
            switch (selectedTab)
            {
                case 0:
                    DrawGameTestTab();
                    break;
                case 1:
                    DrawNetworkDebugTab();
                    break;
                case 2:
                    DrawSceneManagementTab();
                    break;
                case 3:
                    DrawPerformanceTab();
                    break;
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        catch (System.Exception e)
        {
            EditorGUILayout.HelpBox($"GUI错误: {e.Message}", MessageType.Error);
            if (GUILayout.Button("重新加载窗口"))
            {
                Repaint();
            }
        }
    }

    #region 游戏内测试标签页
    private void DrawGameTestTab()
    {
        EditorGUILayout.LabelField("🎮 游戏内测试", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // 子标签页
        gameTestSubTab = GUILayout.Toolbar(gameTestSubTab, gameTestSubTabNames);
        EditorGUILayout.Space(10);
        
        // 根据选中的子标签页显示不同内容
        // 扩展说明：添加新的子标签页时，在此处添加新的case
        switch (gameTestSubTab)
        {
            case 0:
                DrawGameControlSubTab();
                break;
            case 1:
                DrawItemSystemSubTab();
                break;
            case 2:
                DrawUITestSubTab();
                break;
            // 扩展点：添加新的子标签页
            // case 3:
            //     DrawNewFeatureSubTab();
            //     break;
        }
    }
    #endregion
    
    #region 游戏控制子标签页
    private void DrawGameControlSubTab()
    {
        EditorGUILayout.LabelField("🎯 游戏控制", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // 时间控制
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("⏰ 时间控制", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button(isGamePaused ? "▶️ 继续游戏" : "⏸️ 暂停游戏"))
        {
            isGamePaused = !isGamePaused;
            Time.timeScale = isGamePaused ? 0f : timeScale;
            ConsoleManager.LogDebugInfo($"游戏{(isGamePaused ? "暂停" : "继续")}");
        }
        
        if (GUILayout.Button("🔄 重置时间"))
        {
            timeScale = 1.0f;
            Time.timeScale = isGamePaused ? 0f : timeScale;
            ConsoleManager.LogDebugInfo("重置时间倍率");
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        
        GUI.enabled = Application.isPlaying;
        timeScale = EditorGUILayout.Slider("时间倍率", timeScale, 0.1f, 5.0f);
        if (!isGamePaused && Application.isPlaying)
        {
            Time.timeScale = timeScale;
        }
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 角色状态
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("👤 角色状态", EditorStyles.boldLabel);
        
        GUI.enabled = Application.isPlaying;
        godMode = EditorGUILayout.Toggle("🛡️ 无敌模式", godMode);
        infiniteMana = EditorGUILayout.Toggle("💫 无限法力", infiniteMana);
        noCooldown = EditorGUILayout.Toggle("⚡ 无冷却", noCooldown);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("💖 满血"))
        {
            ConsoleManager.LogDebugInfo("执行满血操作");
        }
        if (GUILayout.Button("💀 死亡"))
        {
            ConsoleManager.LogDebugInfo("执行死亡操作");
        }
        EditorGUILayout.EndHorizontal();
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 快速操作
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("⚡ 快速操作", EditorStyles.boldLabel);
        
        GUI.enabled = Application.isPlaying;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🏃 传送到安全点"))
        {
            ConsoleManager.LogDebugInfo("传送到安全点");
        }
        if (GUILayout.Button("🎯 传送到Boss"))
        {
            ConsoleManager.LogDebugInfo("传送到Boss");
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("💎 获得所有技能"))
        {
            ConsoleManager.LogDebugInfo("获得所有技能");
        }
        if (GUILayout.Button("🗡️ 获得所有装备"))
        {
            ConsoleManager.LogDebugInfo("获得所有装备");
        }
        EditorGUILayout.EndHorizontal();
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
    }
    #endregion

    #region UI测试子标签页
    private void DrawUITestSubTab()
    {
        EditorGUILayout.LabelField("🖥️ UI测试", EditorStyles.boldLabel);
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
            ConsoleManager.LogDebugInfo("打开背包界面");
        }
        if (GUILayout.Button("⚔️ 打开技能"))
        {
            ConsoleManager.LogDebugInfo("打开技能界面");
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🏪 打开商店"))
        {
            ConsoleManager.LogDebugInfo("打开商店界面");
        }
        if (GUILayout.Button("📜 打开任务"))
        {
            ConsoleManager.LogDebugInfo("打开任务界面");
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("⚙️ 打开设置"))
        {
            ConsoleManager.LogDebugInfo("打开设置界面");
        }
        if (GUILayout.Button("❌ 关闭所有UI"))
        {
            ConsoleManager.LogDebugInfo("关闭所有UI");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // UI测试
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🧪 UI测试", EditorStyles.boldLabel);
        
        if (GUILayout.Button("💬 显示测试弹窗"))
        {
            ConsoleManager.LogDebugInfo("显示测试弹窗");
        }
        
        if (GUILayout.Button("📱 切换分辨率"))
        {
            ConsoleManager.LogDebugInfo("切换分辨率");
        }
        
        if (GUILayout.Button("🌙 切换主题"))
        {
            ConsoleManager.LogDebugInfo("切换主题");
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
            ConsoleManager.LogDebugInfo(isConnected ? "连接到服务器" : "断开服务器连接");
        }
        if (GUILayout.Button("🔄 重连"))
        {
            ConsoleManager.LogDebugInfo("重新连接服务器");
        }
        EditorGUILayout.EndHorizontal();
        
        autoReconnect = EditorGUILayout.Toggle("🔄 自动重连", autoReconnect);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 网络状态
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("📊 网络状态", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField($"连接状态: {(isConnected ? "已连接" : "未连接")}");
            EditorGUILayout.LabelField($"延迟: {networkLatency:F1}ms");
            EditorGUILayout.LabelField($"上传速度: {uploadSpeed:F1}KB/s");
            EditorGUILayout.LabelField($"下载速度: {downloadSpeed:F1}KB/s");
        }
        else
        {
            EditorGUILayout.LabelField("连接状态: 未连接 (游戏未运行)");
            EditorGUILayout.LabelField("延迟: 0ms (游戏未运行)");
            EditorGUILayout.LabelField("上传速度: 0KB/s (游戏未运行)");
            EditorGUILayout.LabelField("下载速度: 0KB/s (游戏未运行)");
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 网络测试
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🧪 网络测试", EditorStyles.boldLabel);
        
        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button("📤 发送测试数据"))
        {
            ConsoleManager.LogDebugInfo("发送测试数据包");
        }
        
        if (GUILayout.Button("📥 模拟网络延迟"))
        {
            ConsoleManager.LogDebugInfo("模拟网络延迟");
        }
        
        if (GUILayout.Button("💥 模拟网络断开"))
        {
            ConsoleManager.LogDebugInfo("模拟网络断开");
        }
        GUI.enabled = true;
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
        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button("🎬 跳转场景"))
        {
            string targetScene = sceneNames[selectedSceneIndex];
            ConsoleManager.LogDebugInfo($"跳转到场景: {targetScene}");
            
            // 实现真正的场景跳转
            if (Application.isPlaying)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
            }
        }
        if (GUILayout.Button("🔄 重新加载当前场景"))
        {
            ConsoleManager.LogDebugInfo("重新加载当前场景");
            
            // 重新加载当前场景
            if (Application.isPlaying)
            {
                string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
            }
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 场景信息
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("📋 场景信息", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField($"当前场景: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            EditorGUILayout.LabelField($"场景对象数量: {GetSceneObjectCount()}");
            EditorGUILayout.LabelField($"内存使用: {currentMemoryUsage}MB");
        }
        else
        {
            EditorGUILayout.LabelField("当前场景: 编辑器模式");
            EditorGUILayout.LabelField("场景对象数量: 0 (游戏未运行)");
            EditorGUILayout.LabelField("内存使用: 0MB (游戏未运行)");
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 场景操作
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("⚙️ 场景操作", EditorStyles.boldLabel);
        
        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button("🗑️ 清理场景垃圾"))
        {
            ConsoleManager.LogDebugInfo("清理场景垃圾");
        }
        
        if (GUILayout.Button("💾 保存场景"))
        {
            ConsoleManager.LogDebugInfo("保存场景");
        }
        
        if (GUILayout.Button("📊 场景性能分析"))
        {
            ConsoleManager.LogDebugInfo("开始场景性能分析");
        }
        GUI.enabled = true;
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
        
        showMemoryUsage = EditorGUILayout.Toggle("💾 显示内存使用", showMemoryUsage);
        logPerformance = EditorGUILayout.Toggle("📝 记录性能日志", logPerformance);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 性能数据
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("📊 实时数据", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField($"FPS: {currentFPS:F1}");
            EditorGUILayout.LabelField($"帧时间: {currentFrameTime:F2}ms");
            EditorGUILayout.LabelField($"内存使用: {currentMemoryUsage}MB");
            EditorGUILayout.LabelField($"DrawCall: {currentDrawCalls}");
            EditorGUILayout.LabelField($"三角面数: {currentTriangles:N0}");
        }
        else
        {
            EditorGUILayout.LabelField("FPS: 0 (游戏未运行)");
            EditorGUILayout.LabelField("帧时间: 0ms (游戏未运行)");
            EditorGUILayout.LabelField("内存使用: 0MB (游戏未运行)");
            EditorGUILayout.LabelField("DrawCall: 0 (游戏未运行)");
            EditorGUILayout.LabelField("三角面数: 0 (游戏未运行)");
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 性能操作
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🔧 性能操作", EditorStyles.boldLabel);
        
        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button("🗑️ 强制垃圾回收"))
        {
            System.GC.Collect();
            ConsoleManager.LogDebugInfo("执行垃圾回收");
        }
        
        if (GUILayout.Button("📊 生成性能报告"))
        {
            ConsoleManager.LogDebugInfo("生成性能报告");
        }
        
        if (GUILayout.Button("⚡ 优化场景"))
        {
            ConsoleManager.LogDebugInfo("执行场景优化");
        }
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
    }
    #endregion

    #region 道具系统子标签页
    private void DrawItemSystemSubTab()
    {
        EditorGUILayout.LabelField("🎒 道具系统", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // 角色属性
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("👤 角色属性", EditorStyles.boldLabel);
        
        goldAmount = EditorGUILayout.IntField("💰 金币数量", goldAmount);
        expAmount = EditorGUILayout.IntField("⭐ 经验值", expAmount);
        level = EditorGUILayout.IntField("📈 等级", level);
        
        GUI.enabled = Application.isPlaying;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("💰 增加金币"))
        {
            goldAmount += 1000;
            ConsoleManager.LogDebugInfo($"增加1000金币，当前: {goldAmount}");
        }
        if (GUILayout.Button("⭐ 增加经验"))
        {
            expAmount += 100;
            ConsoleManager.LogDebugInfo($"增加100经验，当前: {expAmount}");
        }
        EditorGUILayout.EndHorizontal();
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 道具操作
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🎁 道具操作", EditorStyles.boldLabel);
        
        unlockAllItems = EditorGUILayout.Toggle("🔓 解锁所有道具", unlockAllItems);
        
        GUI.enabled = Application.isPlaying;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("⚔️ 获得武器"))
        {
            ConsoleManager.LogDebugInfo("获得随机武器");
        }
        if (GUILayout.Button("🛡️ 获得防具"))
        {
            ConsoleManager.LogDebugInfo("获得随机防具");
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("💊 获得药水"))
        {
            ConsoleManager.LogDebugInfo("获得治疗药水");
        }
        if (GUILayout.Button("📜 获得卷轴"))
        {
            ConsoleManager.LogDebugInfo("获得魔法卷轴");
        }
        EditorGUILayout.EndHorizontal();
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 快速设置
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("⚡ 快速设置", EditorStyles.boldLabel);
        
        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button("💎 获得所有稀有道具"))
        {
            ConsoleManager.LogDebugInfo("获得所有稀有道具");
        }
        
        if (GUILayout.Button("🗑️ 清空背包"))
        {
            ConsoleManager.LogDebugInfo("清空背包");
        }
        
        if (GUILayout.Button("🔄 重置角色数据"))
        {
            goldAmount = 1000;
            expAmount = 100;
            level = 1;
            ConsoleManager.LogDebugInfo("重置角色数据");
        }
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
    }
    #endregion
}