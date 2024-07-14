using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1601 // Partial elements should be documented

namespace ZodiacBuddy.Stages.Atma.Excel;

[Sheet("MonsterNoteTarget", columnHash: 0x4157404f)]
public class MonsterNoteTarget : ExcelRow {
    public class MonsterNoteTargetPlaceName {
        public LazyRow<PlaceName> Zone { get; set; }

        public LazyRow<PlaceName> Location { get; set; }
    }

    public LazyRow<BNpcName> BNpcName { get; set; }

    public int Icon { get; set; }

    public LazyRow<Town> Town { get; set; }

    public MonsterNoteTargetPlaceName[] PlaceName { get; set; }

    public override void PopulateData(RowParser parser, GameData gameData, Language language) {
        base.PopulateData(parser, gameData, language);

        this.BNpcName = new LazyRow<BNpcName>(gameData, parser.ReadColumn<ushort>(0), language);
        this.Icon = parser.ReadColumn<int>(1);
        this.Town = new LazyRow<Town>(gameData, parser.ReadColumn<byte>(2), language);
        this.PlaceName = new MonsterNoteTargetPlaceName[3];
        for (var i = 0; i < 3; i++) {
            this.PlaceName[i] = new MonsterNoteTargetPlaceName {
                Zone = new LazyRow<PlaceName>(gameData, parser.ReadColumn<ushort>(3 + (i * 2) + 0), language),
                Location = new LazyRow<PlaceName>(gameData, parser.ReadColumn<ushort>(3 + (i * 2) + 1), language),
            };
        }
    }
}

#pragma warning restore SA1601 // Partial elements should be documented
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore SA1201 // Elements should appear in the correct order
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.