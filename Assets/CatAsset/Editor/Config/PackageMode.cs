
/// <summary>
/// 打包模式
/// </summary>
public enum PackageMode
{
    /// <summary>
    /// 将指定文件夹下的所有asset打包为一个bundle
    /// </summary>
    NAssetToOneBundle,

    /// <summary>
    /// 对指定文件夹下所有一级子目录各自使用NAssetToOneBundle打包为一个bundle
    /// </summary>
    TopDirectoryNAssetToOneBundle,

    /// <summary>
    /// 对指定文件夹下的所有asset各自打包为一个bundle
    /// </summary>
    NAssetToNBundle,
}
