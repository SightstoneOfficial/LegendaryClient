using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.MultiUser
{
    public class Deletable<T> where T : class
    {
        private T instance;
        public Deletable(T instance)
        {
            this.instance = instance;
        }

        public static implicit operator Deletable<T>(T inner)
        {
            return new Deletable<T>(inner);
        }

        public void Delete()
        {
            this.instance = null;
        }

        public T Instance
        {
            get { return this.instance; }
        }
    }
}
