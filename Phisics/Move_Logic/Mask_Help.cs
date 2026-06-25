using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Infrastructure.Phisics.Move_Logic
{
    public class Mask_Help
    {
        private PhysicsSimulation physicsSimulation;
        public void SetUpdateMask(int index, int mask)
        {
            if (index >= 0 && index < _maxMoveCount)
            {
                _mainBufferData[index].MassDragAdditionalFlags.w = mask;
            }
        }

        public int GetUpdateMask(int index)
        {
            return index >= 0 && index < _maxMoveCount ?
                (int)_mainBufferData[index].MassDragAdditionalFlags.w : -1;
        }

    }
}
