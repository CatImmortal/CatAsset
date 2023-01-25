using System.Collections.Generic;
using System.IO;

namespace CatAsset.Runtime
{
    public static partial class CatAssetManager
    {
        /// <summary>
        /// 获取资源组信息
        /// </summary>
        public static GroupInfo GetGroupInfo(string group)
        {
            return CatAssetDatabase.GetGroupInfo(group);
        }

        /// <summary>
        /// 获取所有资源组信息
        /// </summary>
        public static List<GroupInfo> GetAllGroupInfo()
        {
            return CatAssetDatabase.GetAllGroupInfo();
        }

        /// <summary>
        /// 获取指定资源组的更新器
        /// </summary>
        public static GroupUpdater GetGroupUpdater(string group)
        {
            return CatAssetUpdater.GetGroupUpdater(group);
        }
        
        /// <summary>
        /// 更新资源组
        /// </summary>
        public static void UpdateGroup(string group, BundleUpdatedCallback callback)
        {
            CatAssetUpdater.UpdateGroup(group, callback);
        }
        
        /// <summary>
        /// 更新指定的资源包
        /// </summary>
        public static void UpdateBundle(string group, BundleManifestInfo info, BundleUpdatedCallback callback,
            TaskPriority priority = TaskPriority.VeryHeight)
        {
            CatAssetUpdater.UpdateBundle(group,info,callback,priority);
        }

        /// <summary>
        /// 暂停资源组更新
        /// </summary>
        public static void PauseGroupUpdater(string group)
        {
            CatAssetUpdater.PauseGroupUpdate(group, true);
        }
        
        /// <summary>
        /// 恢复资源组更新
        /// </summary>
        public static void ResumeGroupUpdater(string group)
        {
            CatAssetUpdater.PauseGroupUpdate(group, false);
        }

        /// <summary>
        /// 校验所有读写区资源包文件
        /// </summary>
        public static int VerifyRearWriteBundles()
        {
            //需要通过更新修复的资源包文件数
            int count = 0;
            
            foreach (var pair in CatAssetDatabase.GetAllBundleRuntimeInfo())
            {
                BundleRuntimeInfo runtimeInfo = pair.Value;
                if (runtimeInfo.BundleState != BundleRuntimeInfo.State.InReadWrite)
                {
                    //不在读写区 跳过
                    continue;
                }

                bool isVerify = RuntimeUtil.VerifyReadWriteBundle(runtimeInfo.LoadPath, runtimeInfo.Manifest);
                if (isVerify)
                {
                    //校验通过 跳过
                    continue;
                }
                
                //校验未通过
                runtimeInfo.BundleState = BundleRuntimeInfo.State.InRemote;
                count++;
            }

            if (count > 0)
            {
                CatAssetUpdater.GenerateReadWriteManifest();
            }

            return count;
        }

    }
}