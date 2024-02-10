// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;

namespace System.Text
{
    /// <summary>
    /// Helper to allow utilizing stack buffer for conversion to UTF-8. Will
    /// switch to ArrayPool if not given enough memory. As such, make sure to
    /// call Clear() to return any potentially rented buffer after conversion.
    /// </summary>
    internal ref struct ValueUtf8Converter
    {
        private byte[]? arrayToReturnToPool;
        private Span<byte> bytes;

        public ValueUtf8Converter(Span<byte> initialBuffer)
        {
            this.arrayToReturnToPool = null;
            this.bytes = initialBuffer;
        }

        public Span<byte> ConvertAndTerminateString(ReadOnlySpan<char> value)
        {
            int maxSize = checked(Encoding.UTF8.GetMaxByteCount(value.Length) + 1);
            if (this.bytes.Length < maxSize)
            {
                this.Dispose();
                this.arrayToReturnToPool = ArrayPool<byte>.Shared.Rent(maxSize);
                this.bytes = new Span<byte>(this.arrayToReturnToPool);
            }

#if NETCOREAPP3_1_OR_GREATER
            // Grab the bytes and null terminate
            int byteCount = Encoding.UTF8.GetBytes(value, this.bytes);
            this.bytes[byteCount] = 0;
            return this.bytes.Slice(0, byteCount + 1);
#else
            // Grab the bytes and null terminate
            // get fixed byte* from ReadOnlySpan<byte>
            unsafe
            {
                #pragma warning disable SA1519 // Braces should not be omitted from multi-line child statement
                fixed (char* pChars = value)
                fixed (byte* pBytes = this.bytes)
                {
                    int byteCount = Encoding.UTF8.GetBytes(pChars, value.Length, pBytes, this.bytes.Length);
                    this.bytes[byteCount] = 0;
                    return this.bytes.Slice(0, byteCount + 1);
                }
            }
#endif
        }

        public void Dispose()
        {
            byte[]? toReturn = this.arrayToReturnToPool;
            if (toReturn != null)
            {
                this.arrayToReturnToPool = null;
                ArrayPool<byte>.Shared.Return(toReturn);
            }
        }
    }
}