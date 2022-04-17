using System;

namespace Finalspace.Onigiri.Types
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

    public struct ExecutionResult<T>
    {
        public bool Success => Error == null;
        public Exception Error { get; }
        public T Value { get; }

        public ExecutionResult(Exception error)
        {
            Error = error;
            Value = default(T);
        }

        public ExecutionResult(T value)
        {
            Error = null;
            Value = value;
        }
    }
}
