using System;

namespace SafeDeleteLibrary
{
    /// <summary>
    /// Exception thrown when secure deletion operations fail
    /// </summary>
    public class SafeDeleteException : Exception
    {
        /// <summary>
        /// The file or directory path that caused the exception
        /// </summary>
        public string? FilePath { get; }

        /// <summary>
        /// The operation that was being performed when the exception occurred
        /// </summary>
        public string? Operation { get; }

        /// <summary>
        /// Initializes a new instance of SafeDeleteException
        /// </summary>
        public SafeDeleteException() : base() { }

        /// <summary>
        /// Initializes a new instance of SafeDeleteException with a message
        /// </summary>
        /// <param name="message">The error message</param>
        public SafeDeleteException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of SafeDeleteException with a message and inner exception
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public SafeDeleteException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of SafeDeleteException with detailed information
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="filePath">The file or directory path</param>
        /// <param name="operation">The operation being performed</param>
        public SafeDeleteException(string message, string filePath, string operation) : base(message)
        {
            FilePath = filePath;
            Operation = operation;
        }

        /// <summary>
        /// Initializes a new instance of SafeDeleteException with detailed information and inner exception
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="filePath">The file or directory path</param>
        /// <param name="operation">The operation being performed</param>
        /// <param name="innerException">The inner exception</param>
        public SafeDeleteException(string message, string filePath, string operation, Exception innerException) 
            : base(message, innerException)
        {
            FilePath = filePath;
            Operation = operation;
        }

        /// <summary>
        /// Returns a string representation of the exception
        /// </summary>
        /// <returns>A formatted string with exception details</returns>
        public override string ToString()
        {
            var result = base.ToString();
            
            if (!string.IsNullOrEmpty(FilePath))
                result += $"\nFile Path: {FilePath}";
                
            if (!string.IsNullOrEmpty(Operation))
                result += $"\nOperation: {Operation}";
                
            return result;
        }
    }
}
