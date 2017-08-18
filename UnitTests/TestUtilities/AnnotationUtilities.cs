﻿using System;
using System.Collections.Generic;
using Nirvana;
using VariantAnnotation;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Sequence;
using Vcf;
using Vcf.VariantCreator;

namespace UnitTests.TestUtilities
{
    public static class AnnotationUtilities
	{
	    internal static IAnnotatedVariant GetVariant(string cacheFilePrefix, List<string> saPaths,
	        string vcfLine,
	        int variantIndex = 0, bool enableVerboseTranscripts = false)
	    {
	        var annotatedPosition = GetAnnotatedPosition(cacheFilePrefix, saPaths, vcfLine,
	            enableVerboseTranscripts);

	        return annotatedPosition.AnnotatedVariants[variantIndex];
	    }

	    internal static IAnnotatedPosition GetAnnotatedPosition(string cacheFilePrefix, List<string> saPaths,
            string vcfLine, bool enableVerboseTranscripts)
	    {

	        var refMinorProvider = ProviderUtilities.GetRefMinorProvider(saPaths);
	        var annotatorAndRef = GetAnnotatorAndReferenceDict(cacheFilePrefix, saPaths);

	        var annotator = annotatorAndRef.Item1;
            var refNames = annotatorAndRef.Item2;
            var variantFactory = new VariantFactory(refNames,refMinorProvider,enableVerboseTranscripts);

	        var position = VcfReaderUtils.ParseVcfLine(vcfLine,variantFactory,refNames);

	        var annotatedPosition = annotator.Annotate(position);

	        return annotatedPosition;
	    }

        private static Tuple<Annotator,IDictionary<string,IChromosome>> GetAnnotatorAndReferenceDict(string cacheFilePrefix, List<string> saPaths)
        {
            var sequenceFilePath = cacheFilePrefix + ".bases";
            var sequenceProvider = ProviderUtilities.GetSequenceProvider(sequenceFilePath);
            var refNames = sequenceProvider.GetChromosomeDictionary();
            var transcriptAnnotationProvider =
                ProviderUtilities.GetTranscriptAnnotationProvider(cacheFilePrefix, sequenceProvider);
            var saProvider = ProviderUtilities.GetSaProvider(saPaths);
            var conservationProvider =
                ProviderUtilities.GetConservationProvider(saPaths);

            var annotator = new Annotator(transcriptAnnotationProvider, sequenceProvider, saProvider, conservationProvider, null);
            return Tuple.Create(annotator,refNames);
        }
    }
}