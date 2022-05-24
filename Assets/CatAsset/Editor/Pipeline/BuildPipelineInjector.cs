using System;
using System.Reflection;

namespace CatAsset.Editor
{
    /// <summary>
    /// 构建管线参数注入器
    /// </summary>
    public static class BuildPipelineInjector
    {
        /// <summary>
        /// 输入构建管线参数到task中
        /// </summary>
        public static void In(IBuildPipelineTask task)
        {
           Filter(task, BuildPipelineParamAttribute.Property.In,(fi =>
           {
              IBuildPipelineParam param = BuildPipelineRunner.GetParam(fi.FieldType);
              fi.SetValue(task,param);
           }));
        }

        /// <summary>
        /// 将task中的构建管线参数输出
        /// </summary>
        public static void Out(IBuildPipelineTask task)
        {
            Filter(task, BuildPipelineParamAttribute.Property.Out,(fi =>
            {
                IBuildPipelineParam param = (IBuildPipelineParam)fi.GetValue(task);
                BuildPipelineRunner.InjectParam(param);
            }));
        }

        /// <summary>
        /// 筛选参数（实现IBuildPipelineParam接口且标记了BuildPipelineParam特性）
        /// </summary>
        private static void Filter(IBuildPipelineTask task,BuildPipelineParamAttribute.Property propCond,Action<FieldInfo> callback)
        {
            FieldInfo[] fields = task.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (FieldInfo fi in fields)
            {
                if (!typeof(IBuildPipelineParam).IsAssignableFrom(fi.FieldType))
                {
                    //此字段未实现对应接口 跳过
                    continue;
                }
                
                BuildPipelineParamAttribute attr = (BuildPipelineParamAttribute)Attribute.GetCustomAttribute(fi, typeof(BuildPipelineParamAttribute));
                if (attr == null)
                {
                    //此字段未标记对应特性 跳过
                    continue;
                }

                if (attr.ParamProp == propCond || attr.ParamProp == BuildPipelineParamAttribute.Property.InOut)
                {
                    //符合属性条件
                    callback?.Invoke(fi);
                }
            }
        }
    }
}