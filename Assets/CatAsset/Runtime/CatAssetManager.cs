using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CatAsset.Runtime
{
    /// <summary>
    /// CatAsset资源管理器
    /// </summary>
    public static partial class CatAssetManager
    {
        /// <summary>
        /// 加载相关任务运行器
        /// </summary>
        private static TaskRunner loadTaskRunner = new TaskRunner();

        /// <summary>
        /// 卸载相关任务运行器
        /// </summary>
        private static TaskRunner unloadTaskRunner = new TaskRunner();
        
        /// <summary>
        /// 下载相关任务运行器
        /// </summary>
        private static TaskRunner downloadTaskRunner = new TaskRunner();

        /// <summary>
        /// 优先级数量
        /// </summary>
        private static int priorityNum = Enum.GetNames(typeof(TaskPriority)).Length;
        
        /// <summary>
        /// 资源类型->自定义原生资源转换方法
        /// </summary>
        private static Dictionary<Type, CustomRawAssetConverter> converterDict =
            new Dictionary<Type, CustomRawAssetConverter>();

        /// <summary>
        /// 资源加载器类型 -> 资源加载器实例
        /// </summary>
        private static Dictionary<Type, BaseAssetLoader> loaderDict = new Dictionary<Type, BaseAssetLoader>();

        /// <summary>
        /// 当前使用的资源加载器
        /// </summary>
        private static BaseAssetLoader assetLoader;
        
        
        /// <summary>
        /// 资源包卸载延迟时间
        /// </summary>
        public static float UnloadBundleDelayTime { get; set; }

        /// <summary>
        /// 资源卸载延迟时间
        /// </summary>
        public static float UnloadAssetDelayTime { get; set; }
        
        /// <summary>
        /// 同时最大任务运行数量
        /// </summary>
        public static int MaxTaskRunCount
        {
            set
            {
                loadTaskRunner.MaxRunCount = value;
                unloadTaskRunner.MaxRunCount = value;
                downloadTaskRunner.MaxRunCount = value;
            }
        }

        /// <summary>
        /// 设置资源更新Uri前缀，下载资源文件时会以 UpdateUriPrefix/BundleRelativePath 为下载地址
        /// </summary>
        public static string UpdateUriPrefix
        {
            get => CatAssetUpdater.UpdateUriPrefix;
            set => CatAssetUpdater.UpdateUriPrefix = value;
        }

        static CatAssetManager()
        {
            RegisterCustomRawAssetConverter(typeof(Texture2D), (bytes =>
            {
                Texture2D texture2D = new Texture2D(0, 0);
                texture2D.LoadImage(bytes);
                return texture2D;
            }));

            RegisterCustomRawAssetConverter(typeof(Sprite), (bytes =>
            {
                Texture2D texture2D = new Texture2D(0, 0);
                texture2D.LoadImage(bytes);
                Sprite sp = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
                return sp;
            }));

            RegisterCustomRawAssetConverter(typeof(TextAsset), (bytes =>
            {
                string text = Encoding.UTF8.GetString(bytes);
                TextAsset textAsset = new TextAsset(text);
                return textAsset;
            }));
        }

        /// <summary>
        /// 轮询CatAsset资源管理器
        /// </summary>
        public static void Update()
        {
            //每帧开始轮询前 清空计数
            loadTaskRunner.PreUpdate();
            unloadTaskRunner.PreUpdate();
            downloadTaskRunner.PreUpdate();
                
            //按优先级从高到低轮询任务组
            for (int i = priorityNum - 1; i >= 0; i--)
            {
                loadTaskRunner.Update(i);
                unloadTaskRunner.Update(i);
                downloadTaskRunner.Update(i);
            }
        }

        /// <summary>
        /// 设置资源加载器
        /// </summary>
        public static void SetAssetLoader<T>()
        {
            SetAssetLoader(typeof(T));
        }

        /// <summary>
        /// 设置资源加载器
        /// </summary>
        public static void SetAssetLoader(Type type)
        {
            if (!loaderDict.TryGetValue(type,out var loader))
            {
                loader = (BaseAssetLoader)Activator.CreateInstance(type);
                loaderDict.Add(type,loader);
            }

            assetLoader = loader;
        }

        /// <summary>
        /// 获取资源加载器
        /// </summary>
        public static BaseAssetLoader GetAssetLoader()
        {
            return assetLoader;
        }
        
        /// <summary>
        /// 注册自定义原生资源转换方法
        /// </summary>
        public static void RegisterCustomRawAssetConverter(Type type, CustomRawAssetConverter converter)
        {
            converterDict[type] = converter;
        }

        /// <summary>
        /// 获取自定义原生资源转换方法
        /// </summary>
        internal static CustomRawAssetConverter GetCustomRawAssetConverter(Type type)
        {
            converterDict.TryGetValue(type, out CustomRawAssetConverter converter);
            return converter;
        }
        
        
        /// <summary>
        /// 将资源句柄绑定到游戏物体上，会在指定游戏物体销毁时卸载绑定的资源
        /// </summary>
        public static void BindToGameObject(GameObject target, IBindableHandler handler)
        {
            if (target == null || handler == null)
            {
                return;
            }
            
            AssetBinder assetBinder = target.GetOrAddComponent<AssetBinder>();
            assetBinder.BindTo(handler);
        }

        /// <summary>
        /// 将资源句柄绑定到场景上，会在指定场景卸载时卸载绑定的资源
        /// </summary>
        public static void BindToScene(Scene scene, IBindableHandler handler)
        {
            if (scene == default || handler == null)
            {
                return;
            }
            
            if (!scene.isLoaded)
            {
                return;
            }
            
            CatAssetDatabase.AddSceneBindHandler(scene, handler);
        }
        
        /// <summary>
        /// 指定资源是否存在
        /// </summary>
        public static bool HasAsset(string assetName)
        {
            return assetLoader.HasAsset(assetName);
        }

    }
}