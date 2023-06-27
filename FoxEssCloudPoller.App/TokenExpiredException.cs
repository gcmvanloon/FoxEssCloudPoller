﻿using System.Runtime.Serialization;

namespace FoxEssCloudPoller
{
    [Serializable]
    internal class TokenExpiredException : Exception
    {
        public TokenExpiredException()
        {
        }

        public TokenExpiredException(string? message) : base(message)
        {
        }

        public TokenExpiredException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected TokenExpiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}