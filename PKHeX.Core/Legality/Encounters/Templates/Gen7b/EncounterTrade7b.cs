using System;

namespace PKHeX.Core;

/// <summary>
/// Generation 7 LGP/E Trade Encounter
/// </summary>
public sealed record EncounterTrade7b(GameVersion Version) : IEncounterable, IEncounterMatch, IEncounterConvertible<PB7>,
    IFixedTrainer, IFixedIVSet, ITrainerID32ReadOnly
{
    public byte Generation => 7;
    public EntityContext Context => EntityContext.Gen7b;
    public ushort Location => Locations.LinkTrade6NPC;
    public Shiny Shiny => Shiny.Random;
    public bool IsEgg => false;
    public Ball FixedBall => Ball.Poke;
    public bool IsShiny => false;
    public ushort EggLocation => 0;
    public bool IsFixedTrainer => true;
    public AbilityPermission Ability => AbilityPermission.Any12;

    public required ReadOnlyMemory<string> TrainerNames { get; init; }

    public required uint ID32 { get; init; }
    public ushort SID16 => (ushort)(ID32 >> 16);
    public ushort TID16 => (ushort)ID32;
    public required byte OTGender { get; init; }
    public required ushort Species { get; init; }
    public required byte Form { get; init; }
    public required byte Level { get; init; }

    public required IndividualValueSet IVs { get; init; }

    private const string _name = "In-game Trade";
    public string Name => _name;
    public string LongName => _name;
    public byte LevelMin => Level;
    public byte LevelMax => Level;

    #region Generating

    PKM IEncounterConvertible.ConvertToPKM(ITrainerInfo tr) => ConvertToPKM(tr);
    PKM IEncounterConvertible.ConvertToPKM(ITrainerInfo tr, EncounterCriteria criteria) => ConvertToPKM(tr, criteria);

    public PB7 ConvertToPKM(ITrainerInfo tr) => ConvertToPKM(tr, EncounterCriteria.Unrestricted);

    public PB7 ConvertToPKM(ITrainerInfo tr, EncounterCriteria criteria)
    {
        int language = (int)Language.GetSafeLanguage(Generation, (LanguageID)tr.Language);
        var version = this.GetCompatibleVersion(tr.Version);
        var pi = PersonalTable.GG[Species, Form];
        var date = EncounterDate.GetDateSwitch();
        var pk = new PB7
        {
            Species = Species,
            Form = Form,
            CurrentLevel = Level,
            MetLocation = Location,
            MetLevel = Level,
            MetDate = date,
            Ball = (byte)FixedBall,

            ID32 = ID32,
            Version = version,
            Language = language,
            OriginalTrainerGender = OTGender,
            OriginalTrainerName = TrainerNames.Span[language],

            OriginalTrainerFriendship = pi.BaseFriendship,

            Nickname = SpeciesName.GetSpeciesNameGeneration(Species, language, Generation),

            HandlingTrainerName = tr.OT,
            HandlingTrainerGender = tr.Gender,
            CurrentHandler = 1,
            HandlingTrainerFriendship = pi.BaseFriendship,

            ReceivedDate = date,
            ReceivedTime = EncounterDate.GetTime(),
        };

        EncounterUtil.SetEncounterMoves(pk, version, Level);
        pk.ResetHeight();
        pk.ResetWeight();
        pk.ResetCP();
        SetPINGA(pk, criteria, pi);
        pk.ResetPartyStats();

        return pk;
    }

    private void SetPINGA(PB7 pk, in EncounterCriteria criteria, PersonalInfo7GG pi)
    {
        var rnd = Util.Rand;
        pk.PID = rnd.Rand32();
        pk.EncryptionConstant = rnd.Rand32();
        pk.Nature = criteria.GetNature();
        pk.Gender = criteria.GetGender(pi);
        pk.RefreshAbility(criteria.GetAbilityFromNumber(Ability));

        pk.HeightScalar = PokeSizeUtil.GetRandomScalar(rnd);
        pk.WeightScalar = PokeSizeUtil.GetRandomScalar(rnd);
        criteria.SetRandomIVs(pk, IVs);
    }

    #endregion

    #region Matching

    public bool IsTrainerMatch(PKM pk, ReadOnlySpan<char> trainer, int language) => (uint)language < TrainerNames.Length && trainer.SequenceEqual(TrainerNames.Span[language]);

    public bool IsMatchExact(PKM pk, EvoCriteria evo)
    {
        if (pk.MetLevel != Level)
            return false;
        if (IVs.IsSpecified)
        {
            if (!Legal.GetIsFixedIVSequenceValidSkipRand(IVs, pk))
                return false;
        }
        if (pk.ID32 != ID32)
            return false;
        if (evo.Form != Form && !FormInfo.IsFormChangeable(Species, Form, pk.Form, Context, pk.Context))
            return false;
        if (pk.OriginalTrainerGender != OTGender)
            return false;
        if (!IsMatchEggLocation(pk))
            return false;
        return true;
    }

    private bool IsMatchEggLocation(PKM pk)
    {
        var expect = pk is PB8 ? Locations.Default8bNone : EggLocation;
        return pk.EggLocation == expect;
    }

    public EncounterMatchRating GetMatchRating(PKM pk) => EncounterMatchRating.Match;

    #endregion
}
