using System;

namespace Aptiv.Messaging
{
    /// <summary>
    /// Indicates whether the value of the signal will be signed or unsigned.
    /// </summary>
    [Serializable]
    public enum IntDescriptor
    {
        /// <summary>
        /// No sign bits.
        /// </summary>
        Unsigned,
        /// <summary>
        /// msb will be sign bit.
        /// </summary>
        Signed,
    };

    /// <summary>
    /// The order of consecutive bytes in the signal.
    /// </summary>
    [Serializable]
    public enum ByteOrder
    {
        /// <summary>
        /// Big Endian Byte order.
        /// </summary>
        Motorola = 0,
        /// <summary>
        /// Little Endian Byte order.
        /// </summary>
        Intel = 1,
    };

    /// <summary>
    /// Indicates if a message is periodic or not.
    /// </summary>
    [Serializable]
    public enum SendType
    {
        /// <summary>
        /// Message is sent once.
        /// </summary>
        INSTANTANEOUS,
        /// <summary>
        /// Message is sent on an interval.
        /// </summary>
        PERIODIC,
    };
}
