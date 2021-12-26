using System;

namespace Finalspace.Onigiri.Models
{
    public struct ExecutionResult
    {
        public bool Success => Error == null;
        public Exception Error { get; }

        public ExecutionResult(Exception error)
        {
            Error = error;
        }
    }
}
