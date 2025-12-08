using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitecoreCommander.Utils
{
    public static class SequenceGenerator
    {
        // Use an Interlocked operation for thread-safe incrementing of a 32-bit integer.
        // This is the fastest, simplest, and most efficient way to handle counters in C#.
        private static int _sequenceNumber = -1;

        /// <summary>
        /// Resets the sequence counter to -1 so the next call to NextValue will return 0.
        /// </summary>
        public static void Reset()
        {
            // This operation is simple and generally doesn't require complex locking or async overhead.
            Interlocked.Exchange(ref _sequenceNumber, -1);
        }

        /// <summary>
        /// Asynchronously increments the counter and returns the new value.
        /// The first successful call will return 0.
        /// </summary>
        /// <returns>A Task representing the sequence value (0, 1, 2, ...).</returns>
        public static Task<int> NextValue()
        {
            // 1. Thread-Safe Increment: Interlocked.Increment atomically increases the value.
            // It handles race conditions without needing explicit locks or complex await logic.
            int nextValue = Interlocked.Increment(ref _sequenceNumber);

            // 2. Non-Blocking Return: We wrap the completed value in a Task.FromResult.
            // This is highly efficient because no actual asynchronous I/O or long-running work is done;
            // we just need the return type to be compatible with async/await patterns.
            return Task.FromResult(nextValue);
        }
    }
}
