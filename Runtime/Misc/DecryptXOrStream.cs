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
        public override bool CanRead => true;
        public override bool CanSeek => true;
        
        public DecryptXOrStream([NotNull] string path, FileMode mode, FileAccess access) : base(path, mode, access)
        {
        }
        
        public override int Read(byte[] array, int offset, int count)
        {
            long oldPos = Position;
            var byteCount =  base.Read(array, offset, count);

            if (oldPos < EncryptUtil.EncryptBytesLength)
            {
                //读取前的位置 0-63
                
                if (Position < EncryptUtil.EncryptBytesLength)
                {
                    //读取后的位置 0-63
                    //直接将array的前byteCount个字节解密
                    EncryptUtil.EncryptXOr(array,byteCount);
                }
                else
                {
                    //读取后的位置 >=64
                    //解密 从读取前的位置 到 63 的长度的字节
                    //比如读取前的位置60 读取后的位置70 那么就要解密array的前(64 - 60 = 4)个字节
                    long length = EncryptUtil.EncryptBytesLength - oldPos;
                    EncryptUtil.EncryptXOr(array,length);
                }
                
            }
            
            
            
            
            return byteCount;
        }



    }
}