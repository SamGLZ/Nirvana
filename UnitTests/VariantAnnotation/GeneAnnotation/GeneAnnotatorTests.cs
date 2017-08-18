﻿using Moq;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.Interface.GeneAnnotation;
using Xunit;

namespace UnitTests.VariantAnnotation.GeneAnnotation
{
    public sealed class GeneAnnotatorTests
    {
        [Fact]
        public void Annoate_noAnnotation()
        {
            var annotationProvider = new Mock<IGeneAnnotationProvider>();
            var providers = new[] {annotationProvider.Object};
            annotationProvider.Setup(x => x.Annotate(It.IsAny<string>())).Returns((IGeneAnnotation)null);

            var observedResult = GeneAnnotator.Annotate(new[] {"gene1", "gene2"},providers);

            Assert.Equal(0,observedResult.Count);
        }

        [Fact]
        public void Annoate()
        {
            var annotationProvider = new Mock<IGeneAnnotationProvider>();
            var providers = new[] { annotationProvider.Object };
            annotationProvider.Setup(x => x.Annotate("gene2")).Returns((IGeneAnnotation)null);
            var geneAnnotation = new Mock<IGeneAnnotation>();
            annotationProvider.Setup(x => x.Annotate("gene1")).Returns(geneAnnotation.Object);


            var observedResult = GeneAnnotator.Annotate(new[] { "gene1", "gene2" }, providers);

            Assert.Equal(1, observedResult.Count);
            Assert.Equal("gene1",observedResult[0].GeneName);
            Assert.Equal(1, observedResult[0].Annotations.Length);
            Assert.Equal(geneAnnotation.Object,observedResult[0].Annotations[0]);

        }
    }
}