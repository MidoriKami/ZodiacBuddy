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
        { 7863, GetItemName(7863) }, // 7863 Curtana novus
        { 7872, GetItemName(7872) }, // 7872 Holy Shield novus
        { 7865, GetItemName(7865) }, // 7865 Bravura novus
        { 7868, GetItemName(7868) }, // 7868 Thyrse novus
        { 7871, GetItemName(7871) }, // 7871 Omnilex novus
        { 7864, GetItemName(7864) }, // 7864 Sphairai novus
        { 7866, GetItemName(7866) }, // 7866 Gae bolg novus
        { 9253, GetItemName(9253) }, // 9253 Yoshimitsu novus
        { 7867, GetItemName(7867) }, // 7867 Artemis Bow novus
        { 7869, GetItemName(7869) }, // 7869 Stardust Rod novus
        { 7870, GetItemName(7870) }, // 7870 The Veil of Wiyu novus
    };

    private static string GetItemName(uint itemId)
    {
        return Service.DataManager.Excel.GetSheet<Item>()!.GetRow(itemId)!.Name.ToDalamudString().ToString();
    }
}