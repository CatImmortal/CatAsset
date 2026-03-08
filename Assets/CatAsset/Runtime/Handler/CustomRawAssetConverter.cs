using System;
using System.Threading.Tasks;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 自定义原生资源转换器的接口
    /// </summary>
    public interface ICustomRawAssetConverter
    {
        /// <summary>
        /// 转换原生资源数据为指定类型的资源对象
        /// </summary>
        Task<object> Convert(byte[] bytes);
    }

    public abstract class BaseCustomRawAssetConverter<T> : ICustomRawAssetConverter
    {
        /// <inheritdoc cref="ICustomRawAssetConverter.Convert"/>
        public abstract Task<T> Convert(byte[] bytes);
        /// <inheritdoc />
        async Task<object> ICustomRawAssetConverter.Convert(byte[] bytes)
        {
            return await Convert(bytes);
        }
    }


    public delegate T CustomRawAssetConverterFunc<T>(byte[] bytes);

    public delegate
#if !UNITASK
        Task<T>
#else
        UniTask<T>
#endif
        AsyncCustomRawAssetConverterFunc<T>(byte[] bytes);

    internal sealed class AnonymousCustomRawAssetConverter<T> : ICustomRawAssetConverter
    {
        private readonly object convert;

        internal AnonymousCustomRawAssetConverter(AsyncCustomRawAssetConverterFunc<T> convert) =>
            this.convert = convert;

        internal AnonymousCustomRawAssetConverter(CustomRawAssetConverterFunc<T> convert) =>
            this.convert = convert;

        /// <inheritdoc />
        public async Task<object> Convert(byte[] bytes)
        {
            if (convert is AsyncCustomRawAssetConverterFunc<T> asyncConverter)
            {
                return await asyncConverter(bytes);
            }
            else if (convert is CustomRawAssetConverterFunc<T> converter)
            {
                return converter(bytes);
            }

            throw new InvalidOperationException("Invalid converter type");
        }
    }

    public static class CustomRawAssetConverter
    {
        /// <summary>
        /// 创建一个同步的自定义原生资源转换器
        /// </summary>
        public static ICustomRawAssetConverter Create<T>(CustomRawAssetConverterFunc<T> converter)
        {
            _ = converter ?? throw new ArgumentNullException("Cannot create a null converter", nameof(converter));
            return new AnonymousCustomRawAssetConverter<T>(converter);
        }

        /// <summary>
        /// 创建一个支持异步的自定义原生资源转换器，该方法本身不是异步的
        /// </summary>
        public static ICustomRawAssetConverter CreateAsync<T>(AsyncCustomRawAssetConverterFunc<T> converter)
        {
            _ = converter ?? throw new ArgumentNullException("Cannot create a null async converter", nameof(converter));
            return new AnonymousCustomRawAssetConverter<T>(converter);
        }
    }
}
