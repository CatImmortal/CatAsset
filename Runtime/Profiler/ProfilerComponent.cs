using System;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 调试分析器组件
    /// </summary>
    public class ProfilerComponent : MonoBehaviour
    {
        public static Guid MsgSendPlayerToEditor = new Guid("23982ffeaf0c489189579946d8e0840f");
        public static Guid MsgSendEditorToPlayer = new Guid("45e0c47f923142ff847c0d1f8b0554d9");

        /// <summary>
        /// 编辑器下获取分析器信息的回调
        /// </summary>
        public static Action<int, ProfilerInfo> Callback;

        /// <summary>
        /// 数据发送间隔
        /// </summary>
        private const float interval = 1 / 15f;  //1秒发送15次

        /// <summary>
        /// 计时器
        /// </summary>
        private float timer;

        /// <summary>
        /// 要发送的数据类型
        /// </summary>
        public static ProfilerInfoType CurType;

        private void OnEnable()
        {
            PlayerConnection.instance.Register(MsgSendEditorToPlayer, OnEditorMessage);
        }

        private void OnDisable()
        {
            PlayerConnection.instance.Unregister(MsgSendEditorToPlayer, OnEditorMessage);
        }

        /// <summary>
        /// 接收编辑器消息的回调
        /// </summary>
        private void OnEditorMessage(MessageEventArgs arg0)
        {
            byte[] bytes = arg0.data;
            int enumInt = BitConverter.ToInt32(bytes);
            CurType = (ProfilerInfoType)enumInt;
            Debug.Log($"接收编辑器消息切换分析器类型:{CurType}");
        }

        private void Update()
        {
            if (CurType == ProfilerInfoType.None)
            {
                return;
            }

            timer += Time.deltaTime;
            if (timer >= interval)
            {
                timer -= interval;
                var profilerInfo = CatAssetDatabase.GetProfilerInfo(CurType);
#if UNITY_EDITOR
                profilerInfo.RebuildReference();  //重建引用
                Callback?.Invoke(0,profilerInfo);
#else
                PlayerConnection.instance.Send(MsgSendPlayerToEditor, ProfilerInfo.Serialize(profilerInfo));
                ReferencePool.Release(profilerInfo);  //真机下才需要回收对象
#endif


            }


        }
    }
}
