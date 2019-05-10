﻿using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Utilities;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace Nirvana
{
    public static class PreLoadUtilities
    {
        public static IDictionary<IChromosome, List<int>> GetPositions(Stream vcfStream, AnnotationRange annotationRange, ISequenceProvider sequenceProvider)
        {
            var benchmark = new Benchmark();
            Console.Write("Scanning positions required for SA pre-loading....");
            var chromPositions = new Dictionary<IChromosome, List<int>>();

            var refNameToChrom = sequenceProvider.RefNameToChromosome;
            IChromosome chromToAnnotate = null;
            int endPosition = int.MaxValue;
            if (annotationRange != null)
            {
                chromToAnnotate = ReferenceNameUtilities.GetChromosome(refNameToChrom, annotationRange.chromosome);
                endPosition = annotationRange.end;
            }

            using (var reader = new StreamReader(vcfStream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!ReachedAnnotationRange(annotationRange, refNameToChrom, line, chromToAnnotate, out var splits, out IChromosome iChrom)) continue;

                    int position = int.Parse(splits[VcfCommon.PosIndex]);
                    if (position > endPosition) break;

                    string refAllele = splits[VcfCommon.RefIndex];
                    string altAllele = splits[VcfCommon.AltIndex];
                    sequenceProvider.LoadChromosome(iChrom);
                    UpdateChromToPositions(chromPositions, iChrom, position, refAllele, altAllele, sequenceProvider.Sequence);
                }
            }

            int count = SortPositionsAndGetCount(chromPositions);

            Console.WriteLine($"{count} positions found in {Benchmark.ToHumanReadable(benchmark.GetElapsedTime())}");

            return chromPositions;
        }

        public static void UpdateChromToPositions(Dictionary<IChromosome, List<int>> chromPositions, IChromosome chromosome, int position, string refAllele, string altAllele, ISequence refSequence)
        {
            if (!chromPositions.ContainsKey(chromosome)) chromPositions.Add(chromosome, new List<int>(16 * 1024));
            foreach (string allele in altAllele.OptimizedSplit(','))
            {
                if (allele.OptimizedStartsWith('<') || allele.Contains('[') || altAllele.Contains(']')) continue;

                (int shiftedPos, string _, string _) =
                    VariantUtils.TrimAndLeftAlign(position, refAllele, allele, refSequence);
                chromPositions[chromosome].Add(shiftedPos);
            }
        }

        private static int SortPositionsAndGetCount(Dictionary<IChromosome, List<int>> chromPositions)
        {
            var count = 0;
            foreach (var positions in chromPositions.Values)
            {
                positions.Sort();
                count += positions.Count;
            }

            return count;
        }

        private static bool ReachedAnnotationRange(AnnotationRange annotationRange, IDictionary<string, IChromosome> refNameToChrom, string line,
            IChromosome chromToAnnotate, out string[] splits, out IChromosome iChrom)
        {
            splits = null;
            iChrom = null;
            if (line.StartsWith('#')) return false;
            splits = line.Split('\t', 6);

            string chrom = splits[VcfCommon.ChromIndex];

            if (!refNameToChrom.TryGetValue(chrom, out iChrom)) return false;
            if (annotationRange != null && chromToAnnotate != iChrom) return false;
            return true;
        }
    }
}