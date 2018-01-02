﻿using System;
using System.Collections;
using System.IO;

namespace Sightstone.Logic.SWF.SWFTypes
{
    public class Tag
    {
        internal BytecodeHolder byteCode;

        public Tag(byte[] data)
        {
            Data = data;
            byteCode = new BytecodeHolder(this);
        }

        public Tag()
        {
            Data = new byte[0];
            byteCode = new BytecodeHolder(this);
        }

        public byte[] Data { get; internal set; }

        internal int TagCode { get; set; }

        public virtual int ActionRecCount
        {
            get { return 0; }
        }

        public virtual byte[] this[int index]
        {
            get { return null; }
            set { }
        }

        public virtual void ReadData(byte version, BinaryReader binaryReader)
        {
        }

        public TagCodes GetTagCode()
        {
            var val = TagCodes.Unknown;
            if (TagCode != -1)
                val = (TagCodes) Enum.Parse(val.GetType(), TagCode.ToString());

            return val;
        }

        public virtual IEnumerator GetEnumerator()
        {
            return new bytecodeEnumerator(this);
        }

        private class bytecodeEnumerator : IEnumerator
        {
            internal readonly Tag tag;
            private int index = -1;

            internal bytecodeEnumerator(Tag tag)
            {
                this.tag = tag;
                index = -1;
            }

            public byte[] Current
            {
                get
                {
                    if (index >= tag.ActionRecCount) throw new InvalidOperationException();
                    return (tag[index]);
                }
            }

            public void Reset()
            {
                index = -1;
            }

            public bool MoveNext()
            {
                if (index > tag.ActionRecCount) throw new InvalidOperationException();
                return ++index < tag.ActionRecCount;
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        public class BytecodeHolder
        {
            internal readonly Tag tag;

            internal BytecodeHolder(Tag t)
            {
                tag = t;
            }

            public byte[] this[int index]
            {
                get { return tag[index]; }
            }

            public int Count
            {
                get { return tag.ActionRecCount; }
            }
        }
    }
}