using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NLog;

namespace WorkloadTools.Consumer
{
    public abstract class BufferedWorkloadConsumer : WorkloadConsumer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected bool Stopped = false;
        public BlockingCollection<WorkloadEvent> Buffer = new BlockingCollection<WorkloadEvent>(new ConcurrentQueue<WorkloadEvent>(), 100000);
        protected Task BufferReader { get; set; }
        
        public int BufferSize { get; set; } = 100000;

        public override sealed void Consume(WorkloadEvent evt)
        {
            if (evt == null) { return; }
            if (!Buffer.TryAdd(evt, TimeSpan.FromSeconds(5)))
            {
                logger.Error("Failed to add event to buffer within timeout.");
            }

            if (BufferReader == null)
            {
                BufferReader = Task.Factory.StartNew(() => ProcessBuffer());
            }
        }

        protected void ProcessBuffer()
        {
            foreach (var evt in Buffer.GetConsumingEnumerable())
            {
                if (Stopped) { break; }
                ConsumeBuffered(evt);
            }
        }

        protected override void Dispose(bool disposing)
        {
            Stopped = true;
        }

        public abstract void ConsumeBuffered(WorkloadEvent evt);
    }
}
