using System;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包加密设置
    /// </summary>
    [Serializable]
    public enum BundleEncryptOptions : byte
    {
        /// <summary>
        /// 使用全局设置
        /// </summary>
        UseGlobal,
        
        /// <summary>
        /// 不加密
        /// </summary>
        NotEncrypt,

        /// <summary>
        /// 偏移加密
        /// </summary>
        Offset,
        
        /// <summary>
        /// 异或加密
        /// </summary>
        XOr,
    }
}