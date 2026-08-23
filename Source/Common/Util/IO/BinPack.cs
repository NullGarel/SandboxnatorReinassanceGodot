/*
*   2025????????
*   DISCLAIMER: CLANKER GENERATED. POTENTIALLY SLOPPY CODE.
*   AGENT: ChatGPT probably
*/

using MessagePack;
using MessagePack.Resolvers;
using MessagePackGodot;

namespace NullGarel.Util.IO;


/// <summary>
/// No longer used for network DTOs. For that usecase, use <see cref="GodotHelpers.DictPack"/>
/// </summary>
public static class BinPack
{
    private static readonly MessagePackSerializerOptions _options =
        MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(
                GodotResolver.Instance,
                StandardResolver.Instance
            )
        );

    /// <summary>
    /// Converts a custom serializable data type to a byte array
    /// </summary>
    /// <typeparam name="T">The data type, must have the [MessagePackObject] attribute.</typeparam>
    /// <param name="targetData"></param>
    /// <returns>byte array</returns>
    public static byte[] Pack<T>(T targetData) =>
        MessagePackSerializer.Serialize(targetData, _options);

    /// <summary>
    /// From byte array to message pack object.
    /// </summary>
    /// <typeparam name="T">The data type, must have the [MessagePackObject] attribute.</typeparam>
    /// <param name="bytes"></param>
    /// <returns>yes</returns>
    public static T Unpack<T>(byte[] bytes) =>
        MessagePackSerializer.Deserialize<T>(bytes, _options);
}
