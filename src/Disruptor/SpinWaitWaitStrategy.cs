﻿using System.Threading;

namespace Disruptor
{
    /// <summary>
    /// Spin strategy that uses a <see cref="SpinWait"/> for <see cref="IEventProcessor"/>s waiting on a barrier.
    /// <p>
    /// This strategy is a good compromise between performance and CPU resource.
    /// Latency spikes can occur after quiet periods.
    /// </p>
    /// </summary>
    public sealed class SpinWaitWaitStrategy : INonBlockingWaitStrategy
    {
        /// <summary>
        /// <see cref="IWaitStrategy.WaitFor"/>
        /// </summary>
        public WaitResult WaitFor(long sequence, Sequence cursor, ISequence dependentSequence, SequenceBarrierAlert alert)
        {
            long availableSequence;

            var spinWait = new SpinWait();
            while ((availableSequence = dependentSequence.Value) < sequence)
            {
                if (alert.IsActive)
                    return WaitResult.Cancel;

                spinWait.SpinOnce();
            }

            return WaitResult.Success(availableSequence);
        }

        /// <summary>
        /// <see cref="IWaitStrategy.SignalAllWhenBlocking"/>
        /// </summary>
        public void SignalAllWhenBlocking()
        {
        }
    }
}
