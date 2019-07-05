using System;

namespace BlazorStyled
{
    public class StyledException : Exception
    {
        public string Context { get; set; }

        public StyledException(string message, Exception innerException) : base(message, innerException) { }

        public static StyledException GetException(string context, Exception innerException)
        {
            return GetException(context, "CSS Parse Error", innerException);
        }

        public static StyledException GetException(string context, string message, Exception innerException)
        {
            string msg = $"{message}{Environment.NewLine}{context}";
            return new StyledException(msg, innerException) { Context = context };
        }
    }
}