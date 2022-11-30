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

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器下获取分析器信息的回调
        /// </summary>
        public static Action<int, ProfilerInfo> Callback;

        /// <summary>
        /// 编辑器下接收编辑器消息的回调
        /// </summary>
        public static void OnEditorMessage(ProfilerMessageType msgType)
        {
            if (UnityEditor.EditorApplication.isPlaying)
            {
                HandleProfilerMessage(msgType);
            }
        }
#else
        private void OnEnable()
        {
            PlayerConnection.instance.Register(MsgSendEditorToPlayer, OnEditorMessage);
        }

        private void OnDisable()
        {
            PlayerConnection.instance.Unregister(MsgSendEditorToPlayer, OnEditorMessage);
        }

        /// <summary>
        /// 真机下接收编辑器消息的回调
        /// </summary>
        private void OnEditorMessage(MessageEventArgs arg0)
        {
            byte[] bytes = arg0.data;
            var enumInt = BitConverter.ToInt16(bytes,0);
            ProfilerMessageType msgType = (ProfilerMessageType)enumInt;
            HandleProfilerMessage(msgType);
        }
#endif

        /// <summary>
        /// 处理分析器消息
        /// </summary>
        public static void HandleProfilerMessage(ProfilerMessageType msgType)
        {
            switch (msgType)
            {
                case ProfilerMessageType.SampleOnce:
                    Sample();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 采样分析器信息并发送给编辑器
        /// </summary>
        private static void Sample()
        {
            var profilerInfo = CatAssetDatabase.GetProfilerInfo();
#if UNITY_EDITOR
            profilerInfo.RebuildReference();  //重建引用
            Callback?.Invoke(0,profilerInfo);
#else
            PlayerConnection.instance.Send(MsgSendPlayerToEditor, ProfilerInfo.Serialize(profilerInfo));
            ReferencePool.Release(profilerInfo);
#endif

        }
    }
}
