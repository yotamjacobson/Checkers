using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers
{
    class Move
    {
        public int startX, startY, endX, endY, type, eatenType = 0, eatenX, eatenY;
        public bool queen = false;

        public Move(int sx, int sy, int ex, int ey, int type, bool queen = false)
        {
            startX = sx;
            startY = sy;
            endX = ex;
            endY = ey;
            this.type = type;
            this.queen = queen;
        }
        public Move()
        {

        }
    }
}
