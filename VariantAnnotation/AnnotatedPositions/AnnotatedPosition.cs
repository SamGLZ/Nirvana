﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedPosition : IAnnotatedPosition
    {
        public IPosition Position { get; }
        public string CytogeneticBand { get; set; }
        public IAnnotatedVariant[] AnnotatedVariants { get; }
        public IList<IAnnotatedSupplementaryInterval> SupplementaryIntervals { get; } = new List<IAnnotatedSupplementaryInterval>();

        public AnnotatedPosition(IPosition position, IAnnotatedVariant[] annotatedVariants)
        {
            Position          = position;
            AnnotatedVariants = annotatedVariants;
        }

        public string GetJsonString()
        {
            if (AnnotatedVariants == null || AnnotatedVariants.Length == 0) return null;

            var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);

            var originalChromName = Position.VcfFields[0];

            jsonObject.AddStringValue("chromosome",  originalChromName);
            jsonObject.AddIntValue("position",       Position.Start);
	        jsonObject.AddStringValue("repeatUnit",  Position.InfoData.RepeatUnit);
	        jsonObject.AddIntValue("refRepeatCount", Position.InfoData.RefRepeatCount);
			jsonObject.AddIntValue("svEnd",          Position.InfoData.End);
            jsonObject.AddStringValue("refAllele",   Position.RefAllele);
            jsonObject.AddStringValues("altAlleles", Position.AltAlleles);

            jsonObject.AddDoubleValue("quality", Position.Quality);

            jsonObject.AddStringValues("filters", Position.Filters);

            jsonObject.AddIntValues("ciPos",   Position.InfoData.CiPos);
            jsonObject.AddIntValues("ciEnd",   Position.InfoData.CiEnd);
            jsonObject.AddIntValue("svLength", Position.InfoData.SvLength);

            jsonObject.AddDoubleValue("strandBias",             Position.InfoData.StrandBias,"0.#######");
            jsonObject.AddIntValue("jointSomaticNormalQuality", Position.InfoData.JointSomaticNormalQuality);
            jsonObject.AddDoubleValue("recalibratedQuality",    Position.InfoData.RecalibratedQuality);
            jsonObject.AddIntValue("copyNumber",                Position.InfoData.CopyNumber);
            jsonObject.AddBoolValue("colocalizedWithCnv",       Position.InfoData.ColocalizedWithCnv);

	        
			jsonObject.AddStringValue("cytogeneticBand", CytogeneticBand);

            if (Position.Samples != null && Position.Samples.Length > 0) jsonObject.AddStringValues("samples", Position.Samples.Select(s => s.GetJsonString()).ToArray(), false);

            if (SupplementaryIntervals != null && SupplementaryIntervals.Any())
                AddSuppIntervalToJsonObject(jsonObject);

            jsonObject.AddStringValues("variants", AnnotatedVariants.Select(v => v.GetJsonString(originalChromName)).ToArray(), false);

            sb.Append(JsonObject.CloseBrace);
            return sb.ToString();
        }

        private void AddSuppIntervalToJsonObject(JsonObject jsonObject)
        {
            var saDict = new Dictionary<string, List<string>>();
            foreach (var si in SupplementaryIntervals)
            {
                if (!saDict.ContainsKey(si.SupplementaryInterval.KeyName))
                {
                    saDict[si.SupplementaryInterval.KeyName] = new List<string>();
                }
                saDict[si.SupplementaryInterval.KeyName].Add(si.ToString());
            }

            foreach (var kvp in saDict)
            {
                jsonObject.AddStringValues(kvp.Key, kvp.Value.ToArray(), false);
            }
        }
    }
}