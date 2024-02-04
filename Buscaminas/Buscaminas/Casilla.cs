using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buscaminas
{
    internal class Casilla
    {
        public bool bomb { get; set; }
        public int numBombsNext { get; set; }
        public bool discovered { get; set; }
        public bool flag { get; set; }

        public Casilla() 
        { 
            this.bomb = false;
            this.discovered = false;
            this.flag = false;
            this.numBombsNext = 0;
        }
    }
}
