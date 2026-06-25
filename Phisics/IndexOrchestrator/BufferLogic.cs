using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Infrastructure.Phisics.IndexOrchestrator
{
    public static class BufferLogic
    {
        public static Stack<int> indexs {get; private set;}

        pr

        public static void AddIndex(out int index) => indexs.TryPop(out index);

    }
}
