using System;
using System.Collections.Generic;

namespace PKHeX.Core;

public sealed class EvolutionReverseSpecies : IEvolutionReverse
{
    public EvolutionReverseLookup Lineage { get; }

    public EvolutionReverseSpecies(IPersonalTable t, EvolutionMethod[][] entries, ushort maxSpecies)
    {
        var links = BuildLinks(t, entries, maxSpecies);
        Lineage = new EvolutionReverseLookup(links, maxSpecies);
    }

    private static IEnumerable<((ushort Species, byte Form), EvolutionLink Value)> BuildLinks(IPersonalTable t, IReadOnlyList<EvolutionMethod[]> entries, int maxSpecies)
    {
        for (ushort sSpecies = 1; sSpecies <= maxSpecies; sSpecies++)
        {
            var fc = t[sSpecies].FormCount;
            for (byte sForm = 0; sForm < fc; sForm++)
            {
                var index = sSpecies;
                var evos = entries[index];
                foreach (var evo in evos)
                {
                    var dSpecies = evo.Species;
                    if (dSpecies == 0)
                        continue;

                    var dForm = sSpecies == (int)Species.Espurr && evo.Method == EvolutionType.LevelUpFormFemale1 ? (byte)1 : sForm;

                    var link = new EvolutionLink(sSpecies, sForm, evo);
                    yield return ((dSpecies, dForm), link);
                }
            }
        }
    }

    public EvolutionNode GetReverse(ushort species, byte form) => Lineage[species, form];

    public IEnumerable<(ushort Species, byte Form)> GetPreEvolutions(ushort species, byte form)
    {
        var node = Lineage[species, form];

        // No convergent evolutions; first method is enough.
        var s = node.First.Tuple;
        if (s.Species == 0)
            yield break;

        var preEvolutions = GetPreEvolutions(s.Species, s.Form);
        foreach (var preEvo in preEvolutions)
            yield return preEvo;
        yield return s;
    }

    public void BanEvo(ushort species, byte form, Func<PKM, bool> func)
    {
        ref var node = ref Lineage[species, form];
        node.Ban(func);
    }
}
