namespace Assets.Infrastructure.Phisics.Move_Logic
{
    public class Move_Mask_Help
    {
        public Move_Buffers_Logic move { get; private set; }
        public void SetUpdateMask(int index, int mask)
        {
            if (index >= 0 && index < move.mainBufferCount)
            {
                var data = new MainBufferData[1];
                move.MainBuffer1.GetData(data, 0, index, 1);
                data[index].MassDragAdditionalFlags.w = mask;
                move.MainBuffer1.SetData(data, 0, index, 1);
            }
        }

        public int GetUpdateMask(int index)
        {
            var data = new MainBufferData[1];
            move.MainBuffer1.GetData(data, 0, index, 1);
            var mask = data[index].MassDragAdditionalFlags.w;
            return index >= 0 && index < move.mainBufferCount
                ? (int)mask : -1;
        }
    }
}
