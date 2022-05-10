using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建配置
    /// </summary>
    [CreateAssetMenu()]
    public class BundleBuildConfigSO : ScriptableObject
    {
        /// <summary>
        /// 资源包构建目录列表
        /// </summary>
        public List<BundleBuildDirectory> Directories;

        /// <summary>
        /// 资源包构建信息列表
        /// </summary>
        public List<BundleBuildInfo> Bundles;
        
        /// <summary>
        /// 资源包构建规则名->资源包构建规则接口实例
        /// </summary>
        private Dictionary<string, IBundleBuildRule> ruleDict = new Dictionary<string, IBundleBuildRule>();

        /// <summary>
        /// 刷新资源包构建信息
        /// </summary>
        public void RefreshBundleBuildInfo()
        {
            Bundles.Clear();
            
            InitRuleDict();
            for (int i = 0; i < Directories.Count; i++)
            {
                BundleBuildDirectory bundleBuildDirectory = Directories[i];
                IBundleBuildRule rule = ruleDict[bundleBuildDirectory.BuildRuleName];
                List<BundleBuildInfo> bundles = rule.GetBundleList(bundleBuildDirectory);
                Bundles.AddRange(bundles);
            }
        }

        /// <summary>
        /// 初始化资源包构建规则字典
        /// </summary>
        private void InitRuleDict()
        {
            Type[] types = typeof(BundleBuildConfigSO).Assembly.GetTypes();
            foreach (Type type in types)
            {
                if (!type.IsInterface && typeof(IBundleBuildRule).IsAssignableFrom(type) && !ruleDict.ContainsKey(type.Name))
                {
                    IBundleBuildRule rule = (IBundleBuildRule)Activator.CreateInstance(type);
                    ruleDict.Add(type.Name,rule);
                }
            }
        }
    }
}

