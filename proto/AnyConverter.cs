using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Shared;

namespace Proto
{
    /// <summary>
    /// Workaround for the protobuf <see cref="Any.Pack(Google.Protobuf.IMessage)" /> and <see cref="Any.Unpack{T}" /> methods.
    ///
    /// Actually dapr uses JSON to serialize/deserialize <see cref="Any" /> types internally. This isn't standard and so
    /// <see cref="Any.Pack(Google.Protobuf.IMessage)" /> and <see cref="Any.Unpack{T}" /> won't work.
    ///
    /// See: <see href="https://github.com/dapr/dotnet-sdk/issues/268" />
    /// </summary>
    public class AnyConverter
    {
        /// <summary>
        /// Converts the given data to <see cref="Any" />.
        /// </summary>
        /// <param name="data">The data to convert.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <returns>The <see cref="Any" /> representation of the data.</returns>
        public static Any ToAny<T>(T data)
        {
            var any = new Any();
            if (data == null)
                return any;

            var bytes = JsonSerializer.SerializeToUtf8Bytes(data, Config.jsonSerializerOptions);
            any.Value = ByteString.CopyFrom(bytes);

            return any;
        }

        /// <summary>
        /// Converts the given <see cref="Any" /> type to the type `T`.
        /// </summary>
        /// <param name="any">The <see cref="Any" /> type to convert.</param>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <returns>The data as `T`.</returns>
        public static T FromAny<T>(Any any)
        {
            var utf8String = any.Value.ToStringUtf8();
            return JsonSerializer.Deserialize<T>(utf8String, Config.jsonSerializerOptions);
        }
    }
}
