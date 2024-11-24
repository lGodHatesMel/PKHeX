using static PKHeX.Core.LegalityCheckStrings;
using static PKHeX.Core.Ball;

namespace PKHeX.Core;

/// <summary>
/// Verifies the <see cref="PKM.Ball"/> value.
/// </summary>
public sealed class BallVerifier : Verifier
{
    protected override CheckIdentifier Identifier => CheckIdentifier.Ball;

    public override void Verify(LegalityAnalysis data)
    {
        if (data.Entity.Format <= 2)
            return; // no ball info saved
        var result = VerifyBall(data);
        data.AddLine(result);
    }

    private static Ball IsReplacedBall(IVersion enc, PKM pk) => pk switch
    {
        // Trading from PLA origin -> SW/SH will replace the Legends: Arceus ball with a regular Poké Ball
        PK8 when enc.Version == GameVersion.PLA => Poke,

        // No replacement done.
        _ => NoBallReplace,
    };

    private const Ball NoBallReplace = None;

    private CheckResult VerifyBall(LegalityAnalysis data)
    {
        var info = data.Info;
        var enc = info.EncounterOriginal;
        var pk = data.Entity;

        // Capture / Inherit cases -- can be one of many balls
        if (pk.Species == (int)Species.Shedinja && enc.Species != (int)Species.Shedinja) // Shedinja. For Gen3, copy the ball from Nincada
        {
            // Only a Gen3 origin Shedinja can copy the wild ball.
            // Evolution chains will indicate if it could have existed as Shedinja in Gen3.
            // The special move verifier has a similar check!
            if (enc is { Version: GameVersion.HG or GameVersion.SS, IsEgg: false } && pk is { Ball: (int)Sport }) // Can evolve in D/P to retain the HG/SS ball (separate byte) -- not able to be captured in any other ball
                return GetResult(true);
            if (enc.Generation != 3 || info.EvoChainsAllGens.Gen3.Length != 2) // not evolved in Gen3 Nincada->Shedinja
                return VerifyBallEquals(pk, (int)Poke); // Poké Ball Only
        }

        return VerifyBall(pk, enc);
    }

    /// <summary>
    /// Verifies the currently set ball for the <see cref="PKM"/>.
    /// </summary>
    /// <remarks>Call this directly instead of the <see cref="LegalityAnalysis"/> overload if you've already ruled out the above cases needing Evolution chains.</remarks>
    public static BallVerificationResult VerifyBall(PKM pk, IEncounterTemplate enc)
    {
        var ball = IsReplacedBall(enc, pk);
        if (ball != NoBallReplace)
            return VerifyBallEquals(pk, ball);

        // Capturing with Heavy Ball is impossible in Sun/Moon for specific species.
        if (pk is { Ball: (int)Heavy, SM: true } && enc is not EncounterEgg && BallUseLegality.IsAlolanCaptureNoHeavyBall(enc.Species))
            return BadCaptureHeavy; // Heavy Ball, can inherit if from egg (US/UM fixed catch rate calc)

        return enc switch
        {
            EncounterInvalid => GetResult(true), // ignore ball, pass whatever
            EncounterSlot8GO g => GetResult(g.IsBallValid(current, pk.Species, pk)),
            IFixedBall { FixedBall: not None } s => VerifyBallEquals(current, s.FixedBall),
            EncounterSlot8 when pk is IRibbonSetMark8 { RibbonMarkCurry: true } or IRibbonSetAffixed { AffixedRibbon: (sbyte)RibbonIndex.MarkCurry }
                => GetResult(current is Poke or Great or Ultra),

            EncounterEgg => VerifyBallEgg(enc, current, pk), // Inheritance rules can vary.
            EncounterStatic5Entree => VerifyBallEquals(current, BallUseLegality.DreamWorldBalls),
            _ => VerifyBallEquals(current, BallUseLegality.GetWildBalls(enc.Generation, enc.Version)),
        };
    }

    private static BallVerificationResult VerifyBallEgg(PKM pk, IEncounterTemplate enc)
    {
        if (enc.Generation < 6) // No inheriting Balls
            return VerifyBallEquals(ball, Poke); // Must be Poké Ball -- no ball inheritance.

        return ball switch
        {
            (int)Master => BadInheritMaster,
            (int)Cherish => BadInheritCherish,
            _ => VerifyBallInherited(pk, enc),
        };
    }

    private static BallVerificationResult VerifyBallInherited(PKM pk, IEncounterTemplate enc) => enc.Context switch
    {
        EntityContext.Gen6 => VerifyBallEggGen6(pk, enc), // Gen6 Inheritance Rules
        EntityContext.Gen7 => VerifyBallEggGen7(pk, enc), // Gen7 Inheritance Rules
        EntityContext.Gen8 => VerifyBallEggGen8(pk, enc),
        EntityContext.Gen8b => VerifyBallEggGen8BDSP(pk, enc),
        EntityContext.Gen9 => VerifyBallEggGen9(pk, enc),
        _ => BadEncounter,
    };

    private static BallVerificationResult VerifyBallEggGen6(PKM pk, IEncounterTemplate enc)
    {
        if (ball > Dream)
            return GetInvalid(LBallUnavailable);

        var result = BallContext6.Instance.CanBreedWithBall(enc.Species, enc.Form, ball, pk);
        return GetResult(result);
    }

    private static BallVerificationResult VerifyBallEggGen7(PKM pk, IEncounterTemplate enc)
    {
        if (ball > Beast)
            return GetInvalid(LBallUnavailable);

        var result = BallContext7.Instance.CanBreedWithBall(enc.Species, enc.Form, ball, pk);
        return GetResult(result);
    }

    private static BallVerificationResult VerifyBallEggGen8BDSP(PKM pk, IEncounterTemplate enc)
    {
        if (ball > Beast)
            return GetInvalid(LBallUnavailable);

        var species = enc.Species;
        if (species is (int)Species.Spinda) // Can't transfer via HOME.
            return VerifyBallEquals(ball, BallUseLegality.WildPokeBalls4_HGSS);

        var result = BallContextHOME.Instance.CanBreedWithBall(species, enc.Form, ball);
        return GetResult(result);
    }

    private static BallVerificationResult VerifyBallEggGen8(PKM pk, IEncounterTemplate enc)
    {
        if (ball > Beast)
            return GetInvalid(LBallUnavailable);

        var result = BallContextHOME.Instance.CanBreedWithBall(enc.Species, enc.Form, ball);
        return GetResult(result);
    }

    private static BallVerificationResult VerifyBallEggGen9(PKM pk, IEncounterTemplate enc)
    {
        if (ball > Beast)
            return GetInvalid(LBallUnavailable);

        // Paldea Starters: Only via GO (Adventures Abound)
        var species = enc.Species;
        if (species is >= (int)Species.Sprigatito and <= (int)Species.Quaquaval)
            return VerifyBallEquals(ball, BallUseLegality.WildPokeballs8g_WithoutRaid);

        var result = BallContextHOME.Instance.CanBreedWithBall(species, enc.Form, ball);
        return GetResult(result);
    }

    private static BallVerificationResult VerifyBallEquals(PKM pk, byte ball) => GetResult(ball == pk.Ball);
    private static BallVerificationResult VerifyBallEquals(Ball ball, ulong permit) => GetResult(BallUseLegality.IsBallPermitted(permit, (byte)ball));

    private CheckResult GetResult(bool valid) => valid ? GetValid(LBallEnc) : GetInvalid(LBallEncMismatch);

    private CheckResult GetResult(BallInheritanceResult result) => result switch
    {
        BallInheritanceResult.Valid => GetValid(LBallSpeciesPass),
        BallInheritanceResult.BadAbility => GetInvalid(LBallAbility),
        _ => GetInvalid(LBallSpecies),
    };
}
