#region

using System;
using System.Collections;
using System.Globalization;
using System.IO;

#endregion

namespace LegendaryClient.Logic.SWF.SWFTypes
{
    public class Tag
    {
        private BytecodeHolder _bytecode;

        public Tag(byte[] data)
        {
            Data = data;
            _bytecode = new BytecodeHolder(this);
        }

        public Tag()
        {
            Data = new byte[0];
            _bytecode = new BytecodeHolder(this);
        }

        public byte[] Data { get; private set; }

        internal int TagCode { get; set; }

        public virtual int ActionRecCount
        {
            get { return 0; }
        }

        public virtual byte[] this[int index]
        {
            get { return null; }
            set { if (value == null) throw new ArgumentNullException("value"); }
        }

        public virtual void ReadData(byte version, BinaryReader binaryReader)
        {
        }

        public TagCodes GetTagCode()
        {
            var val = TagCodes.Unknown;

            if (TagCode != -1)
                val = (TagCodes) Enum.Parse(val.GetType(), TagCode.ToString(CultureInfo.InvariantCulture));

            return val;
        }

        public virtual IEnumerator GetEnumerator()
        {
            return new BytecodeEnumerator(this);
        }

        public class BytecodeEnumerator : IEnumerator
        {
            private readonly Tag _tag;
            private int _index = -1;

            internal BytecodeEnumerator(Tag tag)
            {
                _tag = tag;
                _index = -1;
            }

            public byte[] Current
            {
                get
                {
                    if (_index >= _tag.ActionRecCount)
                        throw new InvalidOperationException();

                    return (_tag[_index]);
                }
            }

            public void Reset()
            {
                _index = -1;
            }

            public bool MoveNext()
            {
                if (_index > _tag.ActionRecCount)
                    throw new InvalidOperationException();

                return ++_index < _tag.ActionRecCount;
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        public class BytecodeHolder
        {
            private readonly Tag _tag;

            internal BytecodeHolder(Tag t)
            {
                _tag = t;
            }

            public byte[] this[int index]
            {
                get { return _tag[index]; }
            }

            public int Count
            {
                get { return _tag.ActionRecCount; }
            }
        }
    }
}