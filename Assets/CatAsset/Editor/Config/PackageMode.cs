﻿
/// <summary>
/// 打包模式
/// </summary>
public enum PackageMode
{
    /// <summary>
    /// 将指定文件夹下的所有asset打包为一个bundle
    /// </summary>
    Model_1,

    /// <summary>
    /// 对指定文件夹下所有一级子目录各自使用Model_1打包为一个bundle
    /// </summary>
    Model_2,
}