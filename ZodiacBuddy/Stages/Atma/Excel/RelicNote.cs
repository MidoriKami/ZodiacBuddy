using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1601 // Partial elements should be documented

namespace ZodiacBuddy.Excel
{
    [Sheet("RelicNote", columnHash: 0xb557320e)]
    public partial class RelicNote : ExcelRow
    {
        public class RelicNoteMonsterNoteTargetCommon
        {
            public LazyRow<MonsterNoteTarget> MonsterNoteTargetCommon { get; set; }

            public byte MonsterCount { get; set; }
        }

        public class RelicNoteFate
        {
            public LazyRow<Fate> Fate { get; set; }

            public LazyRow<PlaceName> PlaceName { get; set; }
        }

        public LazyRow<EventItem> EventItem { get; set; }

        public RelicNoteMonsterNoteTargetCommon[] MonsterNoteTargetCommon { get; set; }

        public LazyRow<MonsterNoteTarget>[] MonsterNoteTargetNM { get; set; }

        public ushort Unknown24 { get; set; }

        public RelicNoteFate[] Fate { get; set; }

        public LazyRow<Leve>[] Leve { get; set; }

        /// <inheritdoc/>
        public override void PopulateData(RowParser parser, GameData gameData, Language language)
        {
            base.PopulateData(parser, gameData, language);

            this.EventItem = new LazyRow<EventItem>(gameData, parser.ReadColumn<uint>(0), language);
            this.MonsterNoteTargetCommon = new RelicNoteMonsterNoteTargetCommon[10];
            for (var i = 0; i < 10; i++)
            {
                this.MonsterNoteTargetCommon[i] = new RelicNoteMonsterNoteTargetCommon
                {
                    MonsterNoteTargetCommon = new LazyRow<MonsterNoteTarget>(gameData, parser.ReadColumn<ushort>(1 + (i * 2) + 0), language),
                    MonsterCount = parser.ReadColumn<byte>(1 + (i * 2) + 1),
                };
            }

            this.MonsterNoteTargetNM = new LazyRow<MonsterNoteTarget>[3];
            for (var i = 0; i < 3; i++)
                this.MonsterNoteTargetNM[i] = new LazyRow<MonsterNoteTarget>(gameData, parser.ReadColumn<ushort>(21 + i), language);

            this.Unknown24 = parser.ReadColumn<ushort>(24);
            this.Fate = new RelicNoteFate[3];
            for (var i = 0; i < 3; i++)
            {
                this.Fate[i] = new RelicNoteFate
                {
                    Fate = new LazyRow<Fate>(gameData, parser.ReadColumn<ushort>(25 + (i * 2) + 0), language),
                    PlaceName = new LazyRow<PlaceName>(gameData, parser.ReadColumn<ushort>(25 + (i * 2) + 1), language),
                };
            }

            this.Leve = new LazyRow<Leve>[3];
            for (var i = 0; i < 3; i++)
                this.Leve[i] = new LazyRow<Leve>(gameData, parser.ReadColumn<ushort>(31 + i), language);
        }
    }
}

#pragma warning restore SA1601 // Partial elements should be documented
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore SA1201 // Elements should appear in the correct order
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.