﻿/****************************************************************************
 * Description:High-performance byte blocks
 * auto add block and auto reuse block
 * you can directly operate reader or writer's byte array
 * or you can use api: readall()/writeall() to read or write whole bytes in diffrent block
 * Author: hiramtan@live.com
 ****************************************************************************/

using System;
using System.Collections.Generic;

namespace HiSocket
{
    public class ByteBlockBuffer
    {
        public static int Size = 1024;//block's size
        public LinkedList<byte[]> LinkedList { get; private set; }
        public ReadOperator Reader { get; private set; }
        public WriteOperator Writer { get; private set; }
        public ByteBlockBuffer()
        {
            LinkedList = new LinkedList<byte[]>();
            LinkedList.AddFirst(GetBlock());
            Reader = new ReadOperator(this);
            Writer = new WriteOperator(this);
        }
        public bool IsReaderAndWriterInSameNode()
        {
            // if reader and writer in same block, reader's position must not large than writer's position
            return Reader.Node == Writer.Node;
        }
        #region operate all blocks

        public byte[] ReadAllBytes()
        {
            if (IsReaderAndWriterInSameNode())
            {
                var length = Writer.Position - Reader.Position;
                var bytes = new byte[length];
                Array.Copy(Reader.Node.Value, Reader.Position, bytes, 0, length);
                Reader.MovePosition(length);
                return bytes;
            }
            else
            {
                var readerBlockBytes = new byte[Size - Reader.Position];
                Array.Copy(Reader.Node.Value, Reader.Position, readerBlockBytes, 0, readerBlockBytes.Length);
                var betweenReadAndWriterBytes = new List<byte>();
                GetBytesBetweenThoseTwo(Reader.Node, Writer.Node, ref betweenReadAndWriterBytes);
                var writerBlockBytes = new byte[Writer.Position];
                Array.Copy(Writer.Node.Value, 0, writerBlockBytes, 0, writerBlockBytes.Length);
                List<byte> bytes = new List<byte>();
                bytes.AddRange(readerBlockBytes);
                bytes.AddRange(betweenReadAndWriterBytes);
                bytes.AddRange(writerBlockBytes);
                return bytes.ToArray();
            }
        }
        void GetBytesBetweenThoseTwo(LinkedListNode<byte[]> first, LinkedListNode<byte[]> last, ref List<byte> bytes)
        {
            if (first == last)
            {
                throw new Exception("first node and last node is same");
            }
            else if (first.Next == last)
            {
                //0 node between this two blocks
            }
            else
            {
                bytes.AddRange(first.Next.Value);
                GetBytesBetweenThoseTwo(first.Next, last, ref bytes);
            }
        }
        // haven't use for current now
        //void GetHowManyBlockCountBetweenThoseTwo(LinkedListNode<byte[]> first, LinkedListNode<byte[]> last, ref int count)
        //{
        //    if (first == last)
        //    {
        //        throw new Exception("first node and last node is same");
        //    }
        //    else if (first.Next == last)
        //    {
        //        //0 node between this two blocks
        //        count += 0;
        //    }
        //    else
        //    {
        //        count += 1;
        //        GetHowManyBlockCountBetweenThoseTwo(first.Next, last, ref count);
        //    }
        //}


        public void WriteAllBytes(byte[] bytes)
        {
            WriteAllBytes(bytes, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index">how many bytes already been write in</param>
        void WriteAllBytes(byte[] bytes, int index)
        {
            if (index < bytes.Length)
            {
                var canWriteLength = Writer.GetHowManyCountCanWriteInThisBlock();
                var haventWriteLength = bytes.Length - index;
                var reallyWriteLength = haventWriteLength < canWriteLength ? haventWriteLength : canWriteLength;
                Array.Copy(bytes, index, Writer.Node.Value, Writer.Position, reallyWriteLength);
                index += reallyWriteLength;
                Writer.MovePosition(reallyWriteLength);
                WriteAllBytes(bytes, index);
            }
        }
        #endregion
        public byte[] GetBlock()
        {
            return new byte[Size];
        }
    }
}