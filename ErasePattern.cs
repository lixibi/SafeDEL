using System;

namespace SafeDeleteLibrary
{
    /// <summary>
    /// Represents a data erasure pattern used for secure file deletion
    /// </summary>
    internal struct ErasePattern
    {
        /// <summary>
        /// The data pattern to write
        /// </summary>
        public byte[]? Data { get; }

        /// <summary>
        /// Whether this pattern uses repeated data or should generate random data
        /// </summary>
        public bool IsRepeated { get; }

        /// <summary>
        /// Initializes a new ErasePattern with fixed data
        /// </summary>
        /// <param name="data">The fixed data pattern</param>
        public ErasePattern(byte[] data)
        {
            Data = data;
            IsRepeated = true;
        }

        /// <summary>
        /// Initializes a new ErasePattern for random data generation
        /// </summary>
        /// <param name="useRandomData">Must be true to indicate random data</param>
        public ErasePattern(bool useRandomData)
        {
            if (!useRandomData)
                throw new ArgumentException("Use the other constructor for fixed patterns");
            
            Data = null;
            IsRepeated = false;
        }

        /// <summary>
        /// Creates a pattern filled with the specified byte value
        /// </summary>
        /// <param name="pattern">The byte value to repeat</param>
        /// <param name="size">The size of the pattern</param>
        /// <returns>An ErasePattern with the specified repeated byte</returns>
        public static ErasePattern CreatePattern(byte pattern, int size)
        {
            var data = new byte[size];
            for (int i = 0; i < size; i++)
            {
                data[i] = pattern;
            }
            return new ErasePattern(data);
        }

        /// <summary>
        /// Creates a pattern for random data generation
        /// </summary>
        /// <returns>An ErasePattern that will generate random data</returns>
        public static ErasePattern CreateRandomPattern()
        {
            return new ErasePattern(true);
        }
    }
}
