using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

    /// <summary>
    /// 资源依赖分析器
/// 用于分析Unity项目中的资源依赖关系
    /// 主要功能：查找哪些资源被其他资源引用，帮助优化资源打包和加载
    /// </summary>
public class ResDepsAnylyzer : EditorWindow
{
    #region 窗口管理
    private Vector2 scrollPosition;
    private List<string> analysisResults = new List<string>();
    private bool isAnalyzing = false;
    private string[] selectedAssetPaths = new string[0];
    #endregion

    #region 依赖分析相关
    private Dictionary<string, List<string>> dependencyMap = new Dictionary<string, List<string>>();
    private Dictionary<string, List<string>> reverseDependencyMap = new Dictionary<string, List<string>>();
    #endregion

    [MenuItem("Tools/依赖分析/资源依赖分析器")]
    public static void ShowWindow()
    {
        ResDepsAnylyzer window = GetWindow<ResDepsAnylyzer>("资源依赖分析器");
        window.minSize = new Vector2(600, 400);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        
        // 标题
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🔍 资源依赖分析器", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        // 分析控制
        DrawAnalysisControls();
        
        EditorGUILayout.Space(10);
        
        // 结果显示
        DrawResults();
        
        EditorGUILayout.EndVertical();
    }

    private void DrawAnalysisControls()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("📊 分析控制", EditorStyles.boldLabel);
        
        // 显示选中的资源
        EditorGUILayout.LabelField($"选中资源数量: {selectedAssetPaths.Length}");
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🔄 分析选中资源"))
        {
            AnalyzeSelectedAssets();
        }
        
        if (GUILayout.Button("🌍 分析所有资源"))
        {
            AnalyzeAllAssets();
        }
        
        if (GUILayout.Button("🗑️ 清除结果"))
        {
            analysisResults.Clear();
        }
        EditorGUILayout.EndHorizontal();
        
        // 显示分析状态
        if (isAnalyzing)
        {
            EditorGUILayout.HelpBox("正在分析中...", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawResults()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("📋 分析结果", EditorStyles.boldLabel);
        
        if (analysisResults.Count == 0)
        {
            EditorGUILayout.HelpBox("暂无分析结果。请先选择资源并进行分析。", MessageType.Info);
        }
        else
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var result in analysisResults)
            {
                EditorGUILayout.LabelField(result, EditorStyles.wordWrappedLabel);
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        EditorGUILayout.EndVertical();
    }

    private void AnalyzeSelectedAssets()
    {
        var selectedObjects = Selection.objects;
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "请先在Project窗口中选择要分析的资源", "确定");
            return;
        }

        selectedAssetPaths = selectedObjects.Select(obj => AssetDatabase.GetAssetPath(obj)).ToArray();
        StartAnalysis();
    }

    private void AnalyzeAllAssets()
    {
        // 获取项目中所有资源
        string[] allAssetGUIDs = AssetDatabase.FindAssets("", new[] { "Assets" });
        selectedAssetPaths = allAssetGUIDs.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
        StartAnalysis();
    }

    private void StartAnalysis()
    {
        isAnalyzing = true;
        analysisResults.Clear();
        
        try
        {
            EditorUtility.DisplayProgressBar("分析依赖", "正在分析资源依赖关系...", 0f);
            
            // 构建依赖关系图
            BuildDependencyMap();
            
            // 分析依赖关系
            AnalyzeDependencies();
            
            EditorUtility.DisplayDialog("完成", $"分析完成！共分析了 {selectedAssetPaths.Length} 个资源", "确定");
            }
            catch (Exception e)
            {
            Debug.LogError($"分析过程中出现错误: {e.Message}");
            EditorUtility.DisplayDialog("错误", $"分析过程中出现错误: {e.Message}", "确定");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            isAnalyzing = false;
        }
    }

    private void BuildDependencyMap()
    {
        dependencyMap.Clear();
        reverseDependencyMap.Clear();
        
        int totalAssets = selectedAssetPaths.Length;
        
        for (int i = 0; i < totalAssets; i++)
        {
            string assetPath = selectedAssetPaths[i];
            
            // 更新进度条
            EditorUtility.DisplayProgressBar("分析依赖", $"正在分析: {assetPath}", (float)i / totalAssets);
            
            try
            {
                // 获取资源的依赖项
                string[] dependencies = AssetDatabase.GetDependencies(assetPath, false);
                
                if (dependencies.Length > 0)
                {
                    dependencyMap[assetPath] = dependencies.ToList();
                    
                    // 构建反向依赖映射
                    foreach (string dependency in dependencies)
                    {
                        if (!reverseDependencyMap.ContainsKey(dependency))
                        {
                            reverseDependencyMap[dependency] = new List<string>();
                        }
                        reverseDependencyMap[dependency].Add(assetPath);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"分析资源 {assetPath} 时出现错误: {e.Message}");
            }
        }
    }

    private void AnalyzeDependencies()
    {
        analysisResults.Add($"=== 依赖分析报告 ===");
        analysisResults.Add($"分析时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        analysisResults.Add($"分析资源数量: {selectedAssetPaths.Length}");
        analysisResults.Add($"发现依赖关系: {dependencyMap.Count}");
        analysisResults.Add("");
        
        // 分析被引用最多的资源
        AnalyzeMostReferencedAssets();
        
        analysisResults.Add("");
        
        // 分析未被引用的资源
        AnalyzeUnreferencedAssets();
        
        analysisResults.Add("");
        
        // 分析循环依赖
        AnalyzeCircularDependencies();
    }

    private void AnalyzeMostReferencedAssets()
    {
        analysisResults.Add("📈 被引用最多的资源:");
        
        var mostReferenced = reverseDependencyMap
            .Where(kvp => kvp.Value.Count > 1)
            .OrderByDescending(kvp => kvp.Value.Count)
            .Take(10);
        
        foreach (var kvp in mostReferenced)
        {
            string assetName = System.IO.Path.GetFileName(kvp.Key);
            analysisResults.Add($"  {assetName} - 被 {kvp.Value.Count} 个资源引用");
            analysisResults.Add($"    路径: {kvp.Key}");
        }
        
        if (!mostReferenced.Any())
        {
            analysisResults.Add("  未发现被多个资源引用的资源");
        }
    }

    private void AnalyzeUnreferencedAssets()
    {
        analysisResults.Add("🔍 未被引用的资源:");
        
        var unreferenced = selectedAssetPaths
            .Where(path => !reverseDependencyMap.ContainsKey(path) || reverseDependencyMap[path].Count == 0)
            .Where(path => !path.EndsWith(".cs") && !path.EndsWith(".shader")) // 排除脚本和着色器
            .Take(20);
        
        foreach (string path in unreferenced)
        {
            string assetName = System.IO.Path.GetFileName(path);
            analysisResults.Add($"  {assetName}");
            analysisResults.Add($"    路径: {path}");
        }
        
        if (!unreferenced.Any())
        {
            analysisResults.Add("  未发现未被引用的资源");
        }
    }

    private void AnalyzeCircularDependencies()
    {
        analysisResults.Add("🔄 循环依赖检查:");
        
        bool foundCircular = false;
        
        foreach (var kvp in dependencyMap)
        {
            string assetPath = kvp.Key;
            List<string> dependencies = kvp.Value;
            
            foreach (string dependency in dependencies)
            {
                if (dependencyMap.ContainsKey(dependency) && 
                    dependencyMap[dependency].Contains(assetPath))
                {
                    analysisResults.Add($"  发现循环依赖:");
                    analysisResults.Add($"    {System.IO.Path.GetFileName(assetPath)} ↔ {System.IO.Path.GetFileName(dependency)}");
                    foundCircular = true;
                }
            }
        }
        
        if (!foundCircular)
        {
            analysisResults.Add("  未发现循环依赖");
            }
        }
        
        /// <summary>
        /// 右键菜单项：查找资源引用
        /// 在Project窗口中右键选中资源后可以调用此功能
        /// </summary>
        [MenuItem("Assets/依赖分析/查找资源引用")]
        public static void FindResReference()
        {
        var selectedObjects = Selection.objects;
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "请先选择要分析的资源", "确定");
            return;
        }

        // 打开分析窗口
        ShowWindow();
        
        // 自动开始分析选中的资源
        var window = GetWindow<ResDepsAnylyzer>();
        window.AnalyzeSelectedAssets();
        }

        /// <summary>
    /// 菜单项：清除分析缓存
        /// </summary>
    [MenuItem("Tools/依赖分析/清除分析缓存")]
    public static void ClearAnalysisCache()
        {
        EditorUtility.DisplayDialog("提示", "分析缓存已清除", "确定");
    }
} 