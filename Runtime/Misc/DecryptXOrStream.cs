using System;
using System.IO;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 异或解密流
    /// </summary>
    public class DecryptXOrStream : FileStream
    {
        public DecryptXOrStream([NotNull] SafeFileHandle handle, FileAccess access) : base(handle, access)
        {
        }

        public DecryptXOrStream([NotNull] SafeFileHandle handle, FileAccess access, int bufferSize) : base(handle, access, bufferSize)
        {
        }

        public DecryptXOrStream([NotNull] SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync) : base(handle, access, bufferSize, isAsync)
        {
        }

        public DecryptXOrStream(IntPtr handle, FileAccess access) : base(handle, access)
        {
        }

        public DecryptXOrStream(IntPtr handle, FileAccess access, bool ownsHandle) : base(handle, access, ownsHandle)
        {
        }

        public DecryptXOrStream(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize) : base(handle, access, ownsHandle, bufferSize)
        {
        }

        public DecryptXOrStream(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize, bool isAsync) : base(handle, access, ownsHandle, bufferSize, isAsync)
        {
        }

        public DecryptXOrStream([NotNull] string path, FileMode mode) : base(path, mode)
        {
        }

        public DecryptXOrStream([NotNull] string path, FileMode mode, FileAccess access) : base(path, mode, access)
        {
        }

        public DecryptXOrStream([NotNull] string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access, share)
        {
        }

        public DecryptXOrStream([NotNull] string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) : base(path, mode, access, share, bufferSize)
        {
        }

        public DecryptXOrStream([NotNull] string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync) : base(path, mode, access, share, bufferSize, useAsync)
        {
        }

        public DecryptXOrStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options) : base(path, mode, access, share, bufferSize, options)
        {
        }
        
        public override int Read(byte[] array, int offset, int count)
        {
            long oldPos = Position;
            var index =  base.Read(array, offset, count);

            if (oldPos < EncryptUtil.EncryptBytesLength)
            {
                //读取前的位置 0-63
                
                if (Position < EncryptUtil.EncryptBytesLength)
                {
                    //读取后的位置 0-63
                    //直接将array的前count个字节解密
                    EncryptUtil.EncryptXOr(array,count);
                }
                else
                {
                    //读取后的位置 >=64
                    //解密 从读取前的位置 到 63 的长度的字节
                    //比如读取前的位置60 读取后的位置70 那么就要解密array的前(64 - (60 + 1) = 3)个字节
                    long length = EncryptUtil.EncryptBytesLength - (oldPos + 1);
                    EncryptUtil.EncryptXOr(array,length);
                }
                
            }
            
            
            
            
            return index;
        }
    }
}