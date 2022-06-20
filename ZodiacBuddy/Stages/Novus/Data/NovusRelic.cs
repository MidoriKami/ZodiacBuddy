using System.Collections.Generic;

using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;

namespace ZodiacBuddy.Novus.Data;

/// <summary>
/// Define the relic item Id and their names.
/// </summary>
public static class NovusRelic
{
    /// <summary>
    /// List of Novus weapons.
    /// </summary>
    public static readonly Dictionary<uint, string> Novus = new()
    {
        { 7863, GetItemName(7863) }, // Curtana Novus
        { 7872, GetItemName(7872) }, // Holy Shield Novus
        { 7865, GetItemName(7865) }, // Bravura Novus
        { 7868, GetItemName(7868) }, // Thyrse Novus
        { 7871, GetItemName(7871) }, // Omnilex Novus
        { 7864, GetItemName(7864) }, // Sphairai Novus
        { 7866, GetItemName(7866) }, // Gae Bolg Novus
        { 9253, GetItemName(9253) }, // Yoshimitsu Novus
        { 7867, GetItemName(7867) }, // Artemis Bow Novus
        { 7869, GetItemName(7869) }, // Stardust Rod Novus
        { 7870, GetItemName(7870) }, // The Veil of Wiyu Novus
    };

    private static string GetItemName(uint itemId)
    {
        return Service.DataManager.Excel.GetSheet<Item>()!
            .GetRow(itemId)!.Name
            .ToDalamudString()
            .ToString();
    }
}