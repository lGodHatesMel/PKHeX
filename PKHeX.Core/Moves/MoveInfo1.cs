using System;

namespace PKHeX.Core;

/// <summary>
/// Details about moves in <see cref="EntityContext.Gen1"/>
/// </summary>
internal static class MoveInfo1
{
    public static ReadOnlySpan<byte> MovePP_RBY =>
    [
        00, 35, 25, 10, 15, 20, 20, 15, 15, 15, 35, 30, 05, 10, 30, 30, 35, 35, 20, 15,
        20, 20, 10, 20, 30, 05, 25, 15, 15, 15, 25, 20, 05, 35, 15, 20, 20, 20, 15, 30,
        35, 20, 20, 30, 25, 40, 20, 15, 20, 20, 20, 30, 25, 15, 30, 25, 05, 15, 10, 05,
        20, 20, 20, 05, 35, 20, 25, 20, 20, 20, 15, 20, 10, 10, 40, 25, 10, 35, 30, 15,
        20, 40, 10, 15, 30, 15, 20, 10, 15, 10, 05, 10, 10, 25, 10, 20, 40, 30, 30, 20,
        20, 15, 10, 40, 15, 20, 30, 20, 20, 10, 40, 40, 30, 30, 30, 20, 30, 10, 10, 20,
        05, 10, 30, 20, 20, 20, 05, 15, 10, 20, 15, 15, 35, 20, 15, 10, 20, 30, 15, 40,
        20, 15, 10, 05, 10, 30, 10, 15, 20, 15, 40, 40, 10, 05, 15, 10, 10, 10, 15, 30,
        30, 10, 10, 20, 10, 10,
    ];

    public static ReadOnlySpan<byte> MoveType_RBY =>
    [
        00, 00, 00, 00, 00, 00, 00, 09, 14, 12, 00, 00, 00, 00, 00, 00, 00, 02, 00, 02,
        00, 00, 11, 00, 01, 00, 01, 01, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00,
        03, 06, 06, 00, 00, 00, 00, 00, 00, 00, 00, 03, 09, 09, 14, 10, 10, 10, 14, 14,
        13, 10, 14, 00, 02, 02, 01, 01, 01, 01, 00, 11, 11, 11, 00, 11, 11, 03, 11, 11,
        11, 06, 15, 09, 12, 12, 12, 12, 05, 04, 04, 04, 03, 13, 13, 13, 13, 13, 00, 00,
        13, 07, 00, 00, 00, 00, 00, 00, 00, 07, 10, 00, 13, 13, 14, 13, 00, 00, 00, 02,
        00, 00, 07, 03, 03, 04, 09, 10, 10, 00, 00, 00, 00, 13, 13, 00, 01, 00, 13, 03,
        00, 06, 00, 02, 00, 10, 00, 11, 00, 13, 00, 03, 10, 00, 00, 04, 13, 05, 00, 00,
        00, 00, 00, 00, 00, 00,
    ];
}
