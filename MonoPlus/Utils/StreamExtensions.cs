/*﻿ MIT License
   
   Copyright (c) 2022-2024 Chasmical
   
   Permission is hereby granted, free of charge, to any person obtaining a copy
   of this software and associated documentation files (the "Software"), to deal
   in the Software without restriction, including without limitation the rights
   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
   copies of the Software, and to permit persons to whom the Software is
   furnished to do so, subject to the following conditions:
   
   The above copyright notice and this permission notice shall be included in all
   copies or substantial portions of the Software.
*/
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;

namespace MonoPlus;

/// <summary>
///   <para>Provides a set of extension methods for the <see cref="Stream"/> class.</para>
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    ///   <para>Attempts to retrieve the specified <paramref name="stream"/>'s <see cref="Stream.Length"/>, and returns a value indicating whether the operation was successful.</para>
    /// </summary>
    /// <param name="stream">The stream to retrieve the byte length of.</param>
    /// <param name="byteLength">When this method returns, contains the specified <paramref name="stream"/>'s <see cref="Stream.Length"/>, if the operation was successful, or 0 if the operation failed.</param>
    /// <returns><see langword="true"/>, if the specified <paramref name="stream"/>'s <see cref="Stream.Length"/> was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
    [Pure]
    public static bool TryGetLength(this Stream stream, out long byteLength)
    {
        ArgumentNullException.ThrowIfNull(stream);

        try
        {
            byteLength = stream.Length;
            return true;
        }
        catch (NotSupportedException)
        {
            byteLength = 0;
            return false;
        }
    }

    /// <summary>
    ///   <para>Writes the specified <paramref name="stream"/>'s content to a byte array and returns it.</para>
    /// </summary>
    /// <param name="stream">The stream to read the content of.</param>
    /// <returns>A byte array containing the specified <paramref name="stream"/>'s content.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The <paramref name="stream"/>'s content length did not match the retrieved length.</exception>
    /// <exception cref="OverflowException">The <paramref name="stream"/>'s length is greater than <see cref="int.MaxValue"/>.</exception>
    [Pure]
    public static byte[] ToByteArray(this Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.TryGetLength(out long byteLengthLong))
        {
            int byteLength = checked((int)byteLengthLong);
            // If the stream's size is known, allocate and populate an array
#if NET5_0_OR_GREATER
            byte[] array = GC.AllocateUninitializedArray<byte>(byteLength);
#else
                byte[] array = new byte[byteLength];
#endif
            if (stream.Read(array) != byteLength) throw new InvalidOperationException();
            return array;
        }
        // If the stream's size cannot be determined, use a MemoryStream
        MemoryStream temp = new();
        stream.CopyTo(temp);
        return temp.ToArray();
    }


    /// <summary>
    /// Converts current stream to <see cref="T:byte[]"/> asynchronously.
    /// </summary>
    /// <param name="stream">Stream to convert.</param>
    /// <returns><see cref="T:byte[]"/> read from <paramref name="stream"/>.</returns>
    /// <exception cref="InvalidOperationException">readCount doesn't match byteLength</exception>
    [Pure]
    public static async ValueTask<byte[]> ToByteArrayAsync(this Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.TryGetLength(out long byteLengthLong))
        {
            int byteLength = checked((int)byteLengthLong);
            // If the stream's size is known, allocate and populate an array
            byte[] array = GC.AllocateUninitializedArray<byte>(byteLength);
            int readCount = await stream.ReadAsync(array);
            if (readCount != byteLength) throw new InvalidOperationException("readCount doesn't match byteLength");
            return array;
        }
        // If the stream's size cannot be determined, use a MemoryStream
        MemoryStream temp = new();
        await stream.CopyToAsync(temp);
        return temp.ToArray();
    }

    /// <summary>
    ///   <para>Retrieves a byte array containing the specified <paramref name="stream"/>'s content. May return an exposed <see cref="MemoryStream"/>'s buffer, avoiding an allocation.</para>
    /// </summary>
    /// <param name="stream">The stream to read the content of.</param>
    /// <returns>A byte array containing the specified <paramref name="stream"/>'s content.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The <paramref name="stream"/>'s content length did not match the retrieved length.</exception>
    /// <exception cref="OverflowException">The <paramref name="stream"/>'s length is greater than <see cref="int.MaxValue"/>.</exception>
    [Pure]
    public static byte[] ToByteArrayDangerous(this Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        // If the stream is an exposed MemoryStream, try to use its buffer directly without copying
        if (stream is MemoryStream memory && memory.TryGetBuffer(out ArraySegment<byte> segment))
        {
            byte[] array = segment.Array!;
            if (segment.Offset == 0 && segment.Count == array.Length)
                return array;
        }
        // Otherwise, use the safe way with copying instead
        return ToByteArray(stream);
    }

}