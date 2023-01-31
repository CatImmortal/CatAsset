using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace CatAsset.Runtime
{
    /// <summary>
    /// CatAsset资源更新器
    /// </summary>
    public static class CatAssetUpdater
    {
        /// <summary>
        /// 资源更新Uri前缀，下载资源文件时会以 UpdateUriPrefix/BundleRelativePath 为下载地址
        /// </summary>
        internal static string UpdateUriPrefix;

        /// <summary>
        /// 资源组名->资源组对应的资源更新器
        /// </summary>
        internal static readonly Dictionary<string, GroupUpdater> GroupUpdaterDict = new Dictionary<string, GroupUpdater>();


        /// <summary>
        /// 生成读写区资源清单
        /// </summary>
        internal static void GenerateReadWriteManifest()
        {
            //创建 CatAssetManifest
            CatAssetManifest manifest = new CatAssetManifest
            {
                GameVersion = Application.version,
                Platform = Application.platform.ToString()
            };

            //遍历所有资源包 找出存在于读写区的
            List<BundleManifestInfo> bundleInfos = new List<BundleManifestInfo>();
            foreach (KeyValuePair<string,BundleRuntimeInfo> pair in CatAssetDatabase.GetAllBundleRuntimeInfo())
            {
                if (pair.Value.BundleState == BundleRuntimeInfo.State.InReadWrite)
                {
                    bundleInfos.Add(pair.Value.Manifest);
                }
            }
            
            manifest.Bundles = bundleInfos;

            //写入清单文件
            manifest.WriteFile(Application.persistentDataPath,true);
        }

        /// <summary>
        /// 获取指定资源组的更新器，若不存在则添加
        /// </summary>
        internal static GroupUpdater GetOrAddGroupUpdater(string group)
        {
            if (!GroupUpdaterDict.TryGetValue(group,out GroupUpdater updater))
            {
                updater = new GroupUpdater();
                updater.GroupName = group;
                GroupUpdaterDict.Add(group, updater);
            }
            return updater;
        }

        /// <summary>
        /// 获取指定资源组的更新器
        /// </summary>
        internal static GroupUpdater GetGroupUpdater(string group)
        {
            GroupUpdaterDict.TryGetValue(group, out GroupUpdater updater);
            return updater;
        }

        /// <summary>
        /// 删除指定资源组的更新器
        /// </summary>
        internal static void RemoveGroupUpdater(string group)
        {
            GroupUpdaterDict.Remove(group);
        }

        /// <summary>
        /// 清空所有资源组的更新器
        /// </summary>
        internal static void ClearAllGroupUpdater()
        {
            GroupUpdaterDict.Clear();
        }

        /// <summary>
        /// 更新资源组
        /// </summary>
        internal static void UpdateGroup(string group,BundleUpdatedCallback callback,TaskPriority priority = TaskPriority.Middle)
        {
            if (!GroupUpdaterDict.TryGetValue(group,out GroupUpdater groupUpdater))
            {
                Debug.LogError($"更新失败，没有找到该资源组的资源更新器：{group}");
                return;
            }

            //更新指定资源组
            groupUpdater.UpdateGroup(callback,priority);
        }

        /// <summary>
        /// 更新指定的资源包
        /// </summary>
        internal static void UpdateBundle(string group,BundleManifestInfo info, BundleUpdatedCallback callback,
            TaskPriority priority = TaskPriority.VeryHeight)
        {
            if (!GroupUpdaterDict.TryGetValue(group,out GroupUpdater groupUpdater))
            {
                Debug.LogError($"更新失败，没有找到该资源组的资源更新器：{group}");
                return;
            }
            //更新指定资源包
            groupUpdater.UpdateBundle(info,callback,priority);
        }

        /// <summary>
        /// 暂停资源组更新
        /// </summary>
        internal static void PauseGroupUpdate(string group, bool isPause)
        {
            if (!GroupUpdaterDict.TryGetValue(group,out GroupUpdater groupUpdater))
            {
                Debug.LogError($"暂停失败，没有找到该资源组的更新器：{group}");
                return;
            }

            //暂停指定资源组更新器
            if (isPause)
            {
                if (groupUpdater.State == GroupUpdaterState.Running)
                {
                    groupUpdater.State = GroupUpdaterState.Paused;
                }
            }
            else
            {
                if (groupUpdater.State == GroupUpdaterState.Paused)
                {
                    groupUpdater.State = GroupUpdaterState.Running;
                }
            }
        }
    }
}
