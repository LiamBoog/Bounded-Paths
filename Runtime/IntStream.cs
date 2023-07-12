namespace BoundedPaths
{
    public struct IntStream
    {
        private readonly int min;
        private readonly int max;
        private int value;

        /// <summary>
        /// Whether the stream has reached its max value
        /// </summary>
        public bool IsMax => value == max;

        /// <summary>
        /// Create a new stream of ints with the given initial and max values.
        /// </summary>
        /// <param name="value">The value of the first/minimum (inclusive) int in the stream.</param>
        /// <param name="max">The maximum (exclusive) int in the stream.</param>
        public IntStream(int value, int max)
        {
            this.value = value;
            this.max = max - 1;
            min = this.value;
        }

        public static implicit operator int(IntStream stream)
        {
            return stream.value;
        }

        public static int operator +(IntStream stream, int addend)
        {
            int sum = stream.value + addend;
            return sum < stream.max ? sum : stream.max;
        }

        public static int operator -(IntStream stream, int subtrahend)
        {
            int diff = stream.value - subtrahend;
            return diff > stream.min ? diff : stream.min;
        }

        public static IntStream operator ++(IntStream stream)
        {
            if (stream.value < stream.max)
            {
                stream.value++;
            }

            return stream;
        }
    }
}