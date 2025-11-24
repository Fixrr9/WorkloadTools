using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NLog;

using WorkloadTools.Consumer.Analysis;

namespace WorkloadTools.Consumer.Analysis
{
    public class AnalysisConsumer : BufferedWorkloadConsumer
    {
        private WorkloadAnalyzer _analyzer;

        private int _uploadIntervalSeconds;

        public SqlConnectionInfo ConnectionInfo { get; set; }
        public int UploadIntervalSeconds
        {
            get => _uploadIntervalSeconds;
            set
            {
                if (value % 60 != 0)
                {
                    throw new ArgumentOutOfRangeException("UploadIntervalSeconds must be an exact multiple of 60");
                }
                _uploadIntervalSeconds = value;
            }
        }

        public int UploadIntervalMinutes
        {
            get => _uploadIntervalSeconds / 60;
            set => _uploadIntervalSeconds = value * 60;
        }

        public int MaximumWriteRetries { get; set; } = 5;

        public bool SqlNormalizerTruncateTo4000 { get; set; }
        public bool SqlNormalizerTruncateTo1024 { get; set; }

        public bool WriteDetail { get; set; } = true;
        public bool WriteSummary { get; set; } = true;

        public override void ConsumeBuffered(WorkloadEvent evt)
        {
            if(_analyzer == null)
            {
                _analyzer = new WorkloadAnalyzer()
                {
                    Interval = UploadIntervalSeconds / 60,
                    ConnectionInfo = ConnectionInfo,
                    MaximumWriteRetries = MaximumWriteRetries,
                    TruncateTo1024 = SqlNormalizerTruncateTo1024,
                    TruncateTo4000 = SqlNormalizerTruncateTo4000,
                    WriteDetail = WriteDetail
                };
            }

            _analyzer.Add(evt);
        }

        public override bool HasMoreEvents()
        {
            return _analyzer.HasEventsQueued || Buffer.Count > 0;

            
        }

        protected override void Dispose(bool disposing)
        {
            if (_analyzer != null)
            {
                _analyzer.Stop();
                _analyzer.Dispose();
            }
        }

        public override void WaitForCompletion(TimeSpan timeout)
        {
            DateTime start = DateTime.Now;

            Stopped = true;
            while (Buffer.Count > 0 && DateTime.Now - start < timeout)
            {
                Thread.Sleep(100);
            }
        }
    }
}
