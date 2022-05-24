using System;

namespace CatAsset.Editor
{
    /// <summary>
    /// 构建管线参数特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class BuildPipelineParamAttribute : Attribute
    {
        /// <summary>
        /// 属性
        /// </summary>
        public enum Property
        {
            /// <summary>
            /// 注入器会向此参数注入来自前面环节的数据
            /// </summary>
            In,
            
            /// <summary>
            /// 注入器会将此参数输出到后面环节
            /// </summary>
            Out,
            
            /// <summary>
            /// 注入器会同时按照In和Out来处理此参数
            /// </summary>
            InOut,
        }

        /// <summary>
        /// 参数属性
        /// </summary>
        public Property ParamProp;
    }
}