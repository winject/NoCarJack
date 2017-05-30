using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoCarJack
{
    class Timer
    {
        int _limit;
        int _tempLimit;
        bool _expired;

        public int Limit { set { _tempLimit = value; _limit = CitizenFX.Core.Game.GameTime + _tempLimit; } }
        public bool Expired
        {
            get
            {

                if (CitizenFX.Core.Game.GameTime > _limit)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public Timer(int handle)
        {

        }

        public void Reset()
        {
            _limit = 0;
            _tempLimit = 0;
            _expired = false;
        }
    }
}
