using Dalamud.Utility.Signatures;

namespace ZodiacBuddy
{
    /// <summary>
    /// Manager to play sound.
    /// </summary>
    public class Sound
    {
        [Signature("E8 ?? ?? ?? ?? 4D 39 BE")]
        private readonly AlertFuncDelegate playSound = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sound"/> class.
        /// </summary>
        public Sound()
        {
            SignatureHelper.Initialise(this);
        }

        private delegate ulong AlertFuncDelegate(Sounds id, ulong unk1, ulong unk2);

        /// <summary>
        /// List of sounds that can be played.
        /// Correspond of '&lt;se.X&gt;' in-game.
        /// </summary>
        #pragma warning disable format,SA1602
        public enum Sounds : byte
        {
            Sound01 = 0x25,
            Sound02 = 0x26,
            Sound03 = 0x27,
            Sound04 = 0x28,
            Sound05 = 0x29,
            Sound06 = 0x2A,
            Sound07 = 0x2B,
            Sound08 = 0x2C,
            Sound09 = 0x2D,
            Sound10 = 0x2E,
            Sound11 = 0x2F,
            Sound12 = 0x30,
            Sound13 = 0x31,
            Sound14 = 0x32,
            Sound15 = 0x33,
            Sound16 = 0x34,
        }
        #pragma warning restore format,SA1602

        /// <summary>
        /// Play the asked sound.
        /// </summary>
        /// <param name="sound">Sound to play.</param>
        public void PlaySound(Sounds sound)
        {
            this.playSound(sound, 0, 0);
        }
    }
}