using System;
using System.Collections.Generic;

using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;

using static ZodiacBuddy.ZodiacBuddyPlugin;

namespace ZodiacBuddy
{
    /// <summary>
    /// All the static data is stored here for loading.
    /// </summary>
    internal static class Datastore
    {
        /// <summary>
        /// Initializes static members of the <see cref="Datastore"/> class.
        /// </summary>
        static Datastore()
        {
            PluginLog.Debug("Initializing datastore");
            LoadBraveData();
        }

        /// <summary>
        /// Gets the data for the Trial of the Brave books.
        /// </summary>
        public static Dictionary<uint, BraveBook> BraveBooks { get; } = new();

        /// <summary>
        /// Retrieve all the available data about the Trial of the Braves.
        /// </summary>
        private static void LoadBraveData()
        {
            try
            {
                var relicNoteSheet = Service.DataManager.GetExcelSheet<Excel.RelicNote>()!;

                foreach (var bookRow in relicNoteSheet)
                {
                    var eventItem = bookRow.EventItem.Value;
                    if (eventItem == null)
                        continue;

                    var bookID = bookRow.RowId;
                    var bookName = eventItem!.Name.ToString();

                    var enemyCount = bookRow.MonsterNoteTargetCommon.Length;
                    var dungeonCount = bookRow.MonsterNoteTargetNM.Length;
                    var fateCount = bookRow.Fate.Length;
                    var leveCount = bookRow.Leve.Length;

                    // PluginLog.Debug($"Loading book {bookID}: {bookName}");
                    var braveBook = BraveBooks[bookRow.RowId] = new BraveBook()
                    {
                        Name = bookName,
                        Enemies = new BraveTarget[enemyCount],
                        Dungeons = new BraveTarget[dungeonCount],
                        Fates = new BraveTarget[fateCount],
                        Leves = new BraveTarget[leveCount],
                    };

                    for (var i = 0; i < enemyCount; i++)
                    {
                        var mntc = bookRow.MonsterNoteTargetCommon[i].MonsterNoteTargetCommon.Value!;
                        var mntcID = mntc.RowId;

                        var zoneRow = mntc.PlaceName[0].Zone.Value!;
                        var zoneName = zoneRow.Name.ToString();
                        var zoneID = zoneRow.RowId;

                        var locationName = mntc.PlaceName[0].Location.Value!.Name.ToString();

                        var name = mntc.BNpcName.Value!.Singular.ToString();

                        var position = GetMonsterPosition(mntc.RowId);

                        // PluginLog.Debug($"Loaded enemy {mntcID}: {name}");
                        braveBook.Enemies[i] = new BraveTarget()
                        {
                            Name = name,
                            ZoneName = zoneName,
                            ZoneID = zoneID,
                            LocationName = locationName,
                            Position = position,
                        };
                    }

                    for (var i = 0; i < dungeonCount; i++)
                    {
                        var mntc = bookRow.MonsterNoteTargetNM[i].Value!;
                        var mntcID = mntc.RowId;

                        var zoneRow = mntc.PlaceName[0].Zone.Value!;
                        var zoneName = zoneRow.Name.ToString();
                        var zoneID = zoneRow.RowId;

                        var locationName = mntc.PlaceName[0].Location.Value!.Name.ToString();

                        var name = mntc.BNpcName.Value!.Singular;

                        var position = GetMonsterPosition(mntc.RowId);

                        // PluginLog.Debug($"Loaded dungeon {mntcID}: {name}");
                        braveBook.Dungeons[i] = new BraveTarget()
                        {
                            Name = name,
                            ZoneName = zoneName,
                            ZoneID = zoneID,
                            LocationName = locationName,
                            Position = position,
                        };
                    }

                    for (var i = 0; i < fateCount; i++)
                    {
                        var fate = bookRow.Fate[i].Fate.Value!;
                        var fateID = fate.RowId;

                        var position = GetFatePosition(fateID);

                        var zoneName = position.TerritoryType.PlaceName.Value!.Name.ToString();
                        var zoneID = position.TerritoryType.RowId;

                        var name = fate.Name;

                        // PluginLog.Debug($"Loaded fate {fateID}: {name}");
                        braveBook.Fates[i] = new BraveTarget()
                        {
                            Name = name,
                            ZoneName = zoneName,
                            ZoneID = zoneID,
                            LocationName = string.Empty,
                            Position = position,
                        };
                    }

                    for (var i = 0; i < leveCount; i++)
                    {
                        var leve = bookRow.Leve[i].Value!;
                        var leveID = leve.RowId;
                        var leveType = leve.LeveAssignmentType.Value!;
                        var leveName = leve.Name.ToString();
                        var leveClient = leve.LeveClient.Value!.Name.ToString();

                        var position = GetLevelPosition(leveID);

                        var zoneName = position.TerritoryType.PlaceName.Value!.Name.ToString();
                        var zoneID = position.TerritoryType.RowId;

                        var name = $"{leveName} ({leveClient})";

                        // PluginLog.Debug($"Loaded leve {leveID}: {name}");
                        braveBook.Leves[i] = new BraveTarget()
                        {
                            Name = name,
                            ZoneName = zoneName,
                            ZoneID = zoneID,
                            LocationName = string.Empty,
                            Position = position,
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "An error occurred during plugin data load.");
                throw;
            }
        }

        private static MapLinkPayload GetMonsterPosition(uint monsterTargetID)
        {
            return monsterTargetID switch
            {
                #pragma warning disable format
                356 => new MapLinkPayload(152,   5, 29.1f, 15.3f), // sylpheed screech        // East Shroud
                357 => new MapLinkPayload(156,  25, 17.0f, 16.0f), // daring harrier          // Mor Dhona
                358 => new MapLinkPayload(155,  53, 13.8f, 27.0f), // giant logger            // Coerthas Central Highlands
                359 => new MapLinkPayload(138,  18, 17.7f, 16.3f), // shoalspine Sahagin      // Western La Noscea
                360 => new MapLinkPayload(156,  25, 11.0f, 15.1f), // 5th Cohort vanguard     // Mor Dhona
                361 => new MapLinkPayload(180,  30, 23.9f,  7.7f), // synthetic doblyn        // Outer La Noscea
                362 => new MapLinkPayload(140,  20, 11.0f,  6.2f), // 4th Cohort hoplomachus  // Western Thanalan
                363 => new MapLinkPayload(146,  23, 21.5f, 25.2f), // Zanr'ak pugilist        // Southern Thanalan
                364 => new MapLinkPayload(147,  24, 22.1f, 26.6f), // basilisk                // Northern Thanalan
                365 => new MapLinkPayload(137,  17, 29.5f, 20.8f), // 2nd Cohort hoplomachus  // Eastern La Noscea
                366 => new MapLinkPayload(156,  25, 17.0f, 16.0f), // raging harrier          // Mor Dhona
                367 => new MapLinkPayload(155,  53, 13.8f, 30.5f), // biast                   // Coerthas Central Highlands
                368 => new MapLinkPayload(138,  18, 17.6f, 16.0f), // shoaltooth Sahagin      // Western La Noscea
                369 => new MapLinkPayload(146,  23, 21.9f, 18.7f), // tempered gladiator      // Southern Thanalan
                370 => new MapLinkPayload(154,   7, 22.6f, 20.0f), // dullahan                // North Shroud
                371 => new MapLinkPayload(152,   5, 24.2f, 16.9f), // milkroot cluster        // East Shroud
                372 => new MapLinkPayload(138,  18, 13.8f, 16.9f), // shelfscale Reaver       // Western La Noscea
                373 => new MapLinkPayload(146,  23, 26.1f, 21.1f), // Zahar'ak archer         // Southern Thanalan
                374 => new MapLinkPayload(180,  30, 23.9f,  7.7f), // U'Ghamaro golem         // Outer La Noscea
                375 => new MapLinkPayload(155,  53, 33.9f, 21.6f), // Natalan boldwing        // Coerthas Central Highlands
                376 => new MapLinkPayload(153,   6, 30.8f, 24.8f), // wild hog                // South Shroud
                377 => new MapLinkPayload(156,  25, 17.0f, 16.0f), // hexing harrier          // Mor Dhona
                378 => new MapLinkPayload(155,  53, 13.8f, 27.0f), // giant lugger            // Coerthas Central Highlands
                379 => new MapLinkPayload(146,  23, 21.9f, 18.7f), // tempered orator         // Southern Thanalan
                380 => new MapLinkPayload(156,  25, 29.6f, 14.3f), // gigas bonze             // Mor Dhona
                381 => new MapLinkPayload(180,  30, 23.9f,  7.7f), // U'Ghamaro roundsman     // Outer La Noscea
                382 => new MapLinkPayload(152,   5, 24.2f, 17.0f), // sylph bonnet            // East Shroud
                383 => new MapLinkPayload(138,  18, 13.4f, 16.9f), // shelfclaw Reaver        // Western La Noscea
                384 => new MapLinkPayload(146,  23, 26.1f, 21.1f), // Zahar'ak fortune-teller // Southern Thanalan
                385 => new MapLinkPayload(137,  17, 29.5f, 20.8f), // 2nd Cohort laquearius   // Eastern La Noscea
                386 => new MapLinkPayload(138,  18, 18.1f, 19.9f), // shelfscale Sahagin      // Western La Noscea
                387 => new MapLinkPayload(156,  25, 14.1f, 11.0f), // mudpuppy                // Mor Dhona
                388 => new MapLinkPayload(146,  23, 18.9f, 22.9f), // Amalj'aa lancer         // Southern Thanalan
                389 => new MapLinkPayload(156,  25, 25.3f, 10.9f), // lake cobra              // Mor Dhona
                390 => new MapLinkPayload(155,  53, 13.8f, 27.0f), // giant reader            // Coerthas Central Highlands
                391 => new MapLinkPayload(180,  30, 23.9f,  7.7f), // U'Ghamaro quarryman     // Outer La Noscea
                392 => new MapLinkPayload(152,   5, 24.6f, 11.2f), // Sylphlands sentinel     // East Shroud
                393 => new MapLinkPayload(138,  18, 14.4f, 17.0f), // sea wasp                // Western La Noscea
                394 => new MapLinkPayload(147,  24, 18.0f, 16.9f), // magitek vanguard        // Northern Thanalan
                395 => new MapLinkPayload(137,  17, 29.5f, 20.8f), // 2nd Cohort eques        // Eastern La Noscea
                396 => new MapLinkPayload(152,   5, 29.1f, 12.4f), // sylpheed sigh           // East Shroud
                397 => new MapLinkPayload(146,  23, 16.4f, 23.7f), // iron tortoise           // Southern Thanalan
                398 => new MapLinkPayload(156,  25, 11.4f, 12.9f), // 5th Cohort hoplomachus  // Mor Dhona
                399 => new MapLinkPayload(155,  53, 13.8f, 30.5f), // snow wolf               // Coerthas Central Highlands
                400 => new MapLinkPayload(153,   6, 33.3f, 23.7f), // ked                     // South Shroud
                401 => new MapLinkPayload(180,  30, 23.9f,  7.7f), // U'Ghamaro bedesman      // Outer La Noscea
                402 => new MapLinkPayload(138,  18, 13.4f, 16.9f), // shelfeye Reaver         // Western La Noscea
                403 => new MapLinkPayload(140,  20, 11.0f,  6.2f), // 4th Cohort laquearius   // Western Thanalan
                404 => new MapLinkPayload(156,  25, 33.4f, 15.2f), // gigas bhikkhu           // Mor Dhona
                405 => new MapLinkPayload(138,  18, 14.5f, 14.0f), // Sapsa shelfscale        // Western La Noscea
                406 => new MapLinkPayload(146,  23, 18.9f, 22.9f), // Amalj'aa brigand        // Southern Thanalan
                407 => new MapLinkPayload(153,   6, 33.3f, 23.7f), // lesser kalong           // South Shroud
                408 => new MapLinkPayload(156,  25, 11.4f, 12.9f), // 5th Cohort laquearius   // Mor Dhona
                409 => new MapLinkPayload(156,  25, 28.9f, 13.6f), // gigas sozu              // Mor Dhona
                410 => new MapLinkPayload(154,   7, 20.2f, 19.6f), // Ixali windtalon         // North Shroud
                411 => new MapLinkPayload(180,  30, 23.9f,  7.7f), // U'Ghamaro priest        // Outer La Noscea
                412 => new MapLinkPayload(140,  20, 11.0f,  6.2f), // 4th Cohort secutor      // Western Thanalan
                413 => new MapLinkPayload(155,  53, 33.9f, 21.6f), // Natalan watchwolf       // Coerthas Central Highlands
                414 => new MapLinkPayload(152,   5, 24.6f, 11.2f), // violet screech          // East Shroud
                415 => new MapLinkPayload(138,  18, 16.3f, 14.9f), // Sapsa shelfclaw         // Western La Noscea
                416 => new MapLinkPayload(152,   5, 29.1f, 12.4f), // sylpheed snarl          // East Shroud
                417 => new MapLinkPayload(146,  23, 18.9f, 22.9f), // Amalj'aa thaumaturge    // Southern Thanalan
                418 => new MapLinkPayload(156,  25, 11.4f, 12.9f), // 5th Cohort eques        // Mor Dhona
                419 => new MapLinkPayload(138,  18, 16.3f, 14.9f), // Sapsa elbst             // Western La Noscea
                420 => new MapLinkPayload(156,  25, 28.7f,  6.9f), // hippogryph              // Mor Dhona
                421 => new MapLinkPayload(138,  18, 20.4f, 19.1f), // trenchtooth Sahagin     // Western La Noscea
                422 => new MapLinkPayload(155,  53, 33.9f, 21.6f), // Natalan windtalon       // Coerthas Central Highlands
                423 => new MapLinkPayload(180,  30, 23.9f,  7.7f), // elite roundsman         // Outer La Noscea
                424 => new MapLinkPayload(147,  24, 24.8f, 20.8f), // ahriman                 // Northern Thanalan
                427 => new MapLinkPayload(156,  25, 11.4f, 12.9f), // 5th Cohort signifer     // Mor Dhona
                425 => new MapLinkPayload(137,  17, 29.5f, 20.8f), // 2nd Cohort secutor      // Eastern La Noscea
                426 => new MapLinkPayload(156,  25, 31.4f, 14.0f), // gigas shramana          // Mor Dhona
                428 => new MapLinkPayload(152,   5, 28.2f, 17.2f), // dreamtoad               // East Shroud
                429 => new MapLinkPayload(154,   7, 20.2f, 19.6f), // watchwolf               // North Shroud
                430 => new MapLinkPayload(146,  23, 18.9f, 22.9f), // Amalj'aa archer         // Southern Thanalan
                431 => new MapLinkPayload(140,  20, 10.2f,  6.0f), // 4th Cohort signifer     // Western Thanalan
                432 => new MapLinkPayload(146,  23, 31.1f, 19.5f), // Zahar'ak battle drake   // Southern Thanalan
                433 => new MapLinkPayload(138,  18, 16.3f, 14.9f), // Sapsa shelftooth        // Western La Noscea
                434 => new MapLinkPayload(155,  53, 33.9f, 21.6f), // Natalan fogcaller       // Coerthas Central Highlands
                435 => new MapLinkPayload(180,  30, 23.9f,  7.7f), // elite priest            // Outer La Noscea
                436 => new MapLinkPayload(146,  23, 18.9f, 22.9f), // Amalj'aa scavenger      // Southern Thanalan
                437 => new MapLinkPayload(156,  25, 11.4f, 12.9f), // 5th Cohort secutor      // Mor Dhona
                438 => new MapLinkPayload(154,   7, 20.2f, 19.6f), // Ixali boldwing          // North Shroud
                439 => new MapLinkPayload(146,  23, 31.1f, 19.5f), // Zahar'ak pugilist       // Southern Thanalan
                440 => new MapLinkPayload(138,  18, 13.9f, 15.5f), // axolotl                 // Western La Noscea
                441 => new MapLinkPayload(156,  25, 31.0f,  5.6f), // hapalit                 // Mor Dhona
                442 => new MapLinkPayload(180,  30, 23.9f,  7.7f), // elite quarryman         // Outer La Noscea
                443 => new MapLinkPayload(155,  53, 33.9f, 21.6f), // Natalan swiftbeak       // Coerthas Central Highlands
                444 => new MapLinkPayload(152,   5, 23.8f, 14.6f), // violet sigh             // East Shroud
                445 => new MapLinkPayload(137,  17, 29.5f, 20.8f), // 2nd Cohort signifer     // Eastern La Noscea
                446 => new MapLinkPayload(164,   8,  6.8f,  7.6f), // Galvanth the Dominator  // The Tam-Tara Deepcroft
                447 => new MapLinkPayload(168,  37, 11.2f,  6.3f), // Isgebind                // Stone Vigil
                448 => new MapLinkPayload(363, 152, 11.2f, 11.2f), // Diabolos                // The Lost City of Amdapor
                449 => new MapLinkPayload(158,  45, 10.6f,  6.5f), // Aiatar                  // Brayflox's Longstop
                450 => new MapLinkPayload(159,  32, 12.7f,  2.5f), // tonberry king           // The Wanderer's Palace
                451 => new MapLinkPayload(349, 142,  9.2f, 11.3f), // Ouranos                 // Copperbell Mines (Hard)
                452 => new MapLinkPayload(163,  43, 16.0f, 11.2f), // adjudicator             // The Sunken Temple of Qarn
                453 => new MapLinkPayload(350, 138, 11.2f, 11.3f), // Halicarnassus           // Haukke Manor (Hard)
                454 => new MapLinkPayload(360, 145,  6.1f, 11.6f), // Mumuepo the Beholden    // Halatali (Hard)
                455 => new MapLinkPayload(161,  41,  9.2f, 11.3f), // Gyges the Great         // Copperbell Mines
                456 => new MapLinkPayload(171,  86, 12.8f,  7.8f), // Batraal                 // Dzemael Darkhold
                457 => new MapLinkPayload(362, 146, 10.6f,  6.5f), // gobmachine G-VI         // Brayflox's Longstop (Hard)
                458 => new MapLinkPayload(169,   9, 15.6f, 8.30f), // Graffias                // The Thousand Maws of Toto-Rak
                459 => new MapLinkPayload(167,  85, 11.4f, 11.2f), // Anantaboga              // Amdapor Keep
                460 => new MapLinkPayload(170,  97,  7.7f,  7.2f), // chimera                 // Cutter's Cry
                461 => new MapLinkPayload(160, 134, 11.3f, 11.3f), // siren                   // Pharos Sirius
                462 => new MapLinkPayload(157,  31,  4.9f, 17.7f), // Denn the Orcatoothed    // Sastasha
                463 => new MapLinkPayload(172,  38,  3.1f,  8.7f), // Miser's Mistress        // Aurum Vale
                464 => new MapLinkPayload(166,  54, 11.2f, 11.3f), // Lady Amandine           // Haukke Manor
                465 => new MapLinkPayload(162,  46,  6.1f, 11.7f), // Tangata                 // Halatali
                #pragma warning restore format
                _ => throw new ArgumentException($"Unregistered MonsterNoteTarget: {monsterTargetID}"),
            };
        }

        private static MapLinkPayload GetFatePosition(uint fateID)
        {
            return fateID switch
            {
                #pragma warning disable format
                317 => new MapLinkPayload(139, 19, 26.8f, 18.2f), // Surprise                   // Upper La Noscea
                424 => new MapLinkPayload(146, 23, 21.0f, 16.0f), // Heroes of the 2nd          // Southern Thanalan
                430 => new MapLinkPayload(146, 23, 24.0f, 26.0f), // Return to Cinder           // Southern Thanalan
                475 => new MapLinkPayload(155, 53, 34.0f, 13.0f), // Bellyful                   // Coerthas Central Highlands
                480 => new MapLinkPayload(155, 53,  8.0f, 11.0f), // Giant Seps                 // Coerthas Central Highlands
                486 => new MapLinkPayload(155, 53, 10.0f, 28.0f), // Tower of Power             // Coerthas Central Highlands
                493 => new MapLinkPayload(155, 53,  5.0f, 22.0f), // The Taste of Fear          // Coerthas Central Highlands
                499 => new MapLinkPayload(155, 53, 34.0f, 20.0f), // The Four Winds             // Coerthas Central Highlands
                516 => new MapLinkPayload(156, 25, 15.0f, 13.0f), // Black and Nburu            // Mor Dhona
                517 => new MapLinkPayload(156, 25, 13.0f, 12.0f), // Good to Be Bud             // Mor Dhona
                521 => new MapLinkPayload(156, 25, 31.0f,  5.0f), // Another Notch on the Torch // Mor Dhona
                540 => new MapLinkPayload(145, 22, 26.0f, 24.0f), // Quartz Coupling            // Eastern Thanalan
                543 => new MapLinkPayload(145, 22, 30.0f, 25.0f), // The Big Bagoly Theory      // Eastern Thanalan
                552 => new MapLinkPayload(146, 23, 18.0f, 20.0f), // Taken                      // Southern Thanalan
                569 => new MapLinkPayload(138, 18, 21.0f, 19.0f), // Breaching North Tidegate   // Western La Noscea
                571 => new MapLinkPayload(138, 18, 18.0f, 22.0f), // Breaching South Tidegate   // Western La Noscea
                577 => new MapLinkPayload(138, 18, 14.0f, 34.0f), // The King's Justice         // Western La Noscea
                587 => new MapLinkPayload(180, 30, 25.0f, 16.0f), // Schism                     // Outer La Noscea
                589 => new MapLinkPayload(180, 30, 25.0f, 17.0f), // Make It Rain               // Outer La Noscea
                604 => new MapLinkPayload(148, 04, 11.0f, 18.0f), // In Spite of It All         // Central Shroud
                611 => new MapLinkPayload(152, 05, 27.0f, 21.0f), // The Enmity of My Enemy     // East Shroud
                616 => new MapLinkPayload(152, 05, 32.0f, 14.0f), // Breaking Dawn              // East Shroud
                620 => new MapLinkPayload(152, 05, 23.0f, 14.0f), // Everything's Better        // East Shroud
                628 => new MapLinkPayload(154, 07, 21.0f, 19.0f), // What Gored Before          // North Shroud
                632 => new MapLinkPayload(154, 07, 21.0f, 19.0f), // Rude Awakening             // North Shroud
                633 => new MapLinkPayload(154, 07, 19.0f, 20.0f), // Air Supply                 // North Shroud
                642 => new MapLinkPayload(147, 24, 21.0f, 29.0f), // The Ceruleum Road          // Northern Thanalan
                #pragma warning restore format
                _ => throw new ArgumentException($"Unregistered FATE: {fateID}"),
            };
        }

        private static MapLinkPayload GetLevelPosition(uint leveID)
        {
            return leveID switch
            {
                #pragma warning disable format
                643 => new MapLinkPayload(147, 24, 22.0f, 29.0f), // Subduing the Subprime           // Northern Thanalan
                644 => new MapLinkPayload(147, 24, 22.0f, 29.0f), // Necrologos: Pale Oblation       // Northern Thanalan
                645 => new MapLinkPayload(147, 24, 22.0f, 29.0f), // Don't Forget to Cry             // Northern Thanalan
                646 => new MapLinkPayload(147, 24, 22.0f, 29.0f), // Circling the Ceruleum           // Northern Thanalan
                647 => new MapLinkPayload(147, 24, 22.0f, 29.0f), // Someone's in the Doghouse       // Northern Thanalan
                649 => new MapLinkPayload(155, 53, 13.0f, 17.0f), // Necrologos: Whispers of the Gem // Coerthas Central Highlands
                650 => new MapLinkPayload(155, 53, 13.0f, 17.0f), // Got a Gut Feeling about This    // Coerthas Central Highlands
                652 => new MapLinkPayload(155, 53, 13.0f, 17.0f), // The Area's a Bit Sketchy        // Coerthas Central Highlands
                657 => new MapLinkPayload(156, 25, 30.0f, 13.0f), // Necrologos: The Liminal Ones    // Mor Dhona
                658 => new MapLinkPayload(156, 25, 30.0f, 13.0f), // Big, Bad Idea                   // Mor Dhona
                659 => new MapLinkPayload(156, 25, 30.0f, 13.0f), // Put Your Stomp on It            // Mor Dhona
                848 => new MapLinkPayload(155, 53, 12.0f, 17.0f), // Someone's Got a Big Mouth       // Coerthas Central Highlands
                849 => new MapLinkPayload(155, 53, 12.0f, 17.0f), // An Imp Mobile                   // Coerthas Central Highlands
                853 => new MapLinkPayload(155, 53, 12.0f, 17.0f), // Yellow Is the New Black         // Coerthas Central Highlands
                855 => new MapLinkPayload(155, 53, 12.0f, 17.0f), // The Bloodhounds of Coerthas     // Coerthas Central Highlands
                859 => new MapLinkPayload(155, 53, 12.0f, 17.0f), // No Big Whoop                    // Coerthas Central Highlands
                860 => new MapLinkPayload(155, 53, 12.0f, 17.0f), // If You Put It That Way          // Coerthas Central Highlands
                863 => new MapLinkPayload(156, 25, 31.0f, 12.0f), // One Big Problem Solved          // Mor Dhona
                865 => new MapLinkPayload(156, 25, 31.0f, 12.0f), // Go Home to Mama                 // Mor Dhona
                868 => new MapLinkPayload(156, 25, 31.0f, 12.0f), // The Awry Salvages               // Mor Dhona
                870 => new MapLinkPayload(156, 25, 31.0f, 12.0f), // Get off Our Lake                // Mor Dhona
                873 => new MapLinkPayload(156, 25, 31.0f, 12.0f), // Who Writes History              // Mor Dhona
                875 => new MapLinkPayload(156, 25, 31.0f, 12.0f), // The Museum Is Closed            // Mor Dhona
                #pragma warning restore format
                _ => throw new ArgumentException($"Unregistered leve: {leveID}"),
            };
        }
    }
}
