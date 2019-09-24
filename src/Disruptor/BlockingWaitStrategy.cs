﻿using System.Threading;

namespace Disruptor
{
    /// <summary>
    /// Blocking strategy that uses a lock and condition variable for <see cref="IEventProcessor"/>s waiting on a barrier.
    /// 
    /// This strategy should be used when performance and low-latency are not as important as CPU resource.
    /// </summary>
    public sealed class BlockingWaitStrategy : IWaitStrategy
    {
        private readonly object _gate = new object();

        /// <summary>
        /// <see cref="IWaitStrategy.WaitFor"/>
        /// </summary>
        public WaitResult WaitFor(long sequence, Sequence cursor, ISequence dependentSequence, SequenceBarrierAlert alert)
        {
            if (cursor.Value < sequence)
            {
                lock (_gate)
                {
                    while (cursor.Value < sequence)
                    {
                        if (alert.IsActive)
                            return WaitResult.Cancel;

                        Monitor.Wait(_gate);
                    }
                }
            }

            var aggressiveSpinWait = new AggressiveSpinWait();
            long availableSequence;
            while ((availableSequence = dependentSequence.Value) < sequence)
            {
                if (alert.IsActive)
                    return WaitResult.Cancel;

                aggressiveSpinWait.SpinOnce();
            }

            return WaitResult.Success(availableSequence);
        }

        /// <summary>
        /// <see cref="IWaitStrategy.SignalAllWhenBlocking"/>
        /// </summary>
        public void SignalAllWhenBlocking()
        {
            lock (_gate)
            {
                Monitor.PulseAll(_gate);
            }
        }
    }
}
