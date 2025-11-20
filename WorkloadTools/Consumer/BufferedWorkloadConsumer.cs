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

        protected bool stopped = false;
        public BlockingCollection<WorkloadEvent> buffer = new BlockingCollection<WorkloadEvent>(new ConcurrentQueue<WorkloadEvent>(), 100000);
        protected Task BufferReader { get; set; }

        // private SpinWait spin = new SpinWait();
        
        public int BufferSize { get; set; } = 100000;

        public override sealed void Consume(WorkloadEvent evt)
        {
            if (evt == null) { return; }
            if (!buffer.TryAdd(evt, TimeSpan.FromSeconds(5)))
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
            foreach (var evt in buffer.GetConsumingEnumerable())
            {
                if (stopped) { break; }
                ConsumeBuffered(evt);
            }
        }

        protected override void Dispose(bool disposing)
        {
            stopped = true;
        }

        public abstract void ConsumeBuffered(WorkloadEvent evt);
    }
}
