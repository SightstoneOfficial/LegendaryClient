using System;
using System.Collections;
using System.IO;

namespace LegendaryClient.Logic.SWF.SWFTypes
{
    public class Tag
    {
        public byte[] Data { get; private set; }

        internal int TagCode { get; set; }

        public Tag(byte[] data)
        {
            Data = data;
            Bytecode = new BytecodeHolder(this);
        }

        public Tag()
        {
            Data = new byte[0];
            Bytecode = new BytecodeHolder(this);
        }

        public virtual int ActionRecCount
        {
            get
            {
                return 0;
            }
        }

        public virtual byte[] this[int index]
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public virtual void ReadData(byte version, BinaryReader binaryReader)
        {
        }

        private BytecodeHolder Bytecode;

        public TagCodes GetTagCode()
        {
            TagCodes val = TagCodes.Unknown;
            if (this.TagCode != -1)
            {
                val = (TagCodes)System.Enum.Parse(val.GetType(), this.TagCode.ToString());
            }
            return val;
        }

        public virtual IEnumerator GetEnumerator()
        {
            return (IEnumerator)new BytecodeEnumerator(this);
        }

        public class BytecodeHolder
        {
            public byte[] this[int index]
            {
                get
                {
                    return tag[index];
                }
            }

            public int Count
            {
                get
                {
                    return tag.ActionRecCount;
                }
            }

            private Tag tag;

            internal BytecodeHolder(Tag t)
            {
                tag = t;
            }
        }

        public class BytecodeEnumerator : IEnumerator
        {
            private int index = -1;
            private Tag tag;

            internal BytecodeEnumerator(Tag tag)
            {
                this.tag = tag;
                this.index = -1;
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

            public byte[] Current
            {
                get
                {
                    if (index >= tag.ActionRecCount) throw new InvalidOperationException();
                    return (tag[index]);
                }
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }
        }
    }
}