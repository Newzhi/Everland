using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.GUI;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace U3DUdpater.AAHelper.Editor.Performance
{
    /// <summary>
    /// 资源依赖分析器
    /// 继承自BundleRuleBase，用于分析Unity Addressable系统中的资源依赖关系
    /// 主要功能：查找哪些资源被其他资源引用，帮助优化资源打包和加载
    /// </summary>
    public class ResDepsAnylyzer:BundleRuleBase
    {
        /// <summary>
        /// 是否可以自动修复问题（这里设为false，表示需要手动处理）
        /// </summary>
        public override bool CanFix { get { return false; } }
        
        /// <summary>
        /// 分析规则的显示名称
        /// </summary>
        public override string ruleName
        {
            get { return "查找资源依赖"; }
        }

        /// <summary>
        /// 执行依赖分析的主要方法
        /// </summary>
        /// <param name="settings">Addressable资源设置</param>
        /// <returns>分析结果列表</returns>
        public override List<AnalyzeResult> RefreshAnalysis(AddressableAssetSettings settings)
        {
            List<AnalyzeResult> results = new List<AnalyzeResult>();

            // 检查是否有未保存的场景，如果有则提示用户保存
            if (!BuildUtility.CheckModifiedScenesAndAskToSave())
            {
                Debug.LogError("Cannot run Analyze with unsaved scenes");
                results.Add(new AnalyzeResult { resultName = ruleName + "Cannot run Analyze with unsaved scenes" });
                return results;
            }

            // 获取依赖缓存管理器实例
            var depsManager = ResDepsCacheManager.Inst;
            
            // 显示进度条
            EditorUtility.DisplayCancelableProgressBar("分析依赖", "分析依赖", 0);
            try
            {
                // 运行所有依赖分析任务，显示进度
                var runResult = depsManager.RunAllTasks((cur, total) =>
                {
                    // 更新进度条，显示当前进度
                    if (EditorUtility.DisplayCancelableProgressBar("分析依赖", $"分析依赖: {cur}/{total}", (float)cur / total))
                    {
                        return true; // 用户取消了操作
                    }

                    return false; // 继续执行
                });
                
                // 如果任务执行失败，返回现有结果
                if(!runResult)
                {
                    return m_Results;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return m_Results;
            }
            finally
            {
                // 清理进度条
                EditorUtility.ClearProgressBar();
            }
            
            // 获取当前选中的资源GUID列表
            var selections = Selection.assetGUIDs;
            // 获取反向依赖映射（key是资源GUID，value是引用该资源的其他资源GUID列表）
            var ReverseDepsMap = depsManager.ReverseDepsMap;
            
            // 如果没有选中任何资源，则分析所有资源
            if (!selections.Any())
            {
                selections = ReverseDepsMap.Keys.ToArray();
            }

            var now = DateTime.Now;
            // 生成查询ID，用于标识这次分析
            // var qId = $"{now.Year}/{now.Month}/{now.Day}-{now.Hour}.{now.Minute}.{now.Second}.{now.Millisecond}";
            var qId="Query - "+string.Join(", ",selections.Take(5).Select(p=>AssetDatabase.GUIDToAssetPath(p)));
            
            // 添加查询标识结果
            {
                var ret = new AnalyzeResult()
                {
                    resultName = qId,
                    severity = MessageType.None,
                };
                m_Results.Add(ret);
            }
            
            // 遍历所有选中的资源
            foreach (var depInfoKey in selections)
            {
                // 尝试获取该资源的反向依赖信息
                if (ReverseDepsMap.TryGetValue(depInfoKey, out var depInfoValue))
                {
                    // 如果没有其他资源引用这个资源，跳过
                    if (depInfoValue.Count == 0)
                    {
                        continue;
                    }
                    // 如果只有一个引用，且是自己引用自己，跳过
                    if (depInfoValue.Count == 1)
                    {
                        if (depInfoValue.First() == depInfoKey)
                        {
                            continue;
                        }
                    }

                    // 获取当前资源的路径
                    var depPath = AssetDatabase.GUIDToAssetPath(depInfoKey);
                    
                    // 遍历所有引用该资源的其他资源
                    foreach (var dep in depInfoValue)
                    {
                        // 跳过自己引用自己的情况
                        if (dep == depInfoKey)
                        {
                            continue;
                        }
                        
                        // 创建分析结果：显示"被引用资源 -> 引用者"的关系
                        var ret = new AnalyzeResult()
                        {
                            resultName = $"{qId}{kDelimiter}{depPath}{kDelimiter}{AssetDatabase.GUIDToAssetPath(dep)}",
                            severity = MessageType.None,
                        };
                        m_Results.Add(ret);
                    }
                }
                else
                {
                    // 如果在反向依赖映射中找不到该资源，标记为未知
                    var depPath = AssetDatabase.GUIDToAssetPath(depInfoKey);
                    var ret = new AnalyzeResult()
                    {
                        resultName = $"{qId}{kDelimiter}{depPath}{kDelimiter}???",
                        severity = MessageType.None,
                    };
                    m_Results.Add(ret);
                }
            }

            return m_Results;
        }
        
        /// <summary>
        /// 静态初始化类，用于在Unity加载时注册分析规则
        /// </summary>
        [InitializeOnLoad]
        class RegisterResDepsAnylyzerLayout
        {
            static RegisterResDepsAnylyzerLayout()
            {
                // 将当前分析器注册到Addressable分析系统中
                AnalyzeSystem.RegisterNewRule<ResDepsAnylyzer>();
            }
        }
        
        /// <summary>
        /// 右键菜单项：查找资源引用
        /// 在Project窗口中右键选中资源后可以调用此功能
        /// </summary>
        [MenuItem("Assets/依赖分析/查找资源引用")]
        public static void FindResReference()
        {
            // 显示Addressable分析窗口
            AnalyzeWindow.ShowWindow();
            var treeView = AnalyzeSystem.TreeView;
            var rows = treeView.GetRows();
            
            // 查找当前分析器在树视图中的位置
            var container=rows[0].children[1].children.FirstOrDefault(item =>
            {
                return item is AnalyzeRuleContainerTreeViewItem tItem && tItem.analyzeRule is ResDepsAnylyzer;
            })as AnalyzeRuleContainerTreeViewItem;
            
            // 展开树视图节点，显示当前分析器
            treeView.SetExpanded(new List<int>()
            {
                container.parent.parent.id,
                container.parent.id,
                container.id,
            });
            
            // 选中当前分析器并运行分析
            AnalyzeSystem.TreeView.SetSelection(new List<int>(){container.id});
            AnalyzeSystem.TreeView.RunAllSelectedRules();
        }

        /// <summary>
        /// 菜单项：清除资源引用缓存
        /// 用于清除已缓存的依赖分析结果，强制重新分析
        /// </summary>
        [MenuItem("Tools/依赖分析/清除资源引用缓存")]
        public static void ClearResDepsCache()
        {
            ResDepsCacheManager.Inst.ClearResultCache();
        }
    }
} 