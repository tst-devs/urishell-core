using System;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace UriShell.Shell.Resolution
{
    public class UriResolvedMetadataTest
    {
        [Fact]
        public void AssignIdReturnsTheSameMetadataWithTheSpecifiedId()
        {
            var metadata1 = new UriResolvedMetadata(new Uri("about:blank"), Substitute.For<IDisposable>());
            var metadata2 = metadata1.AssignId(1005);

            metadata2.ResolvedId.Should().Be(1005);
            metadata2.Disposable.Should().Be(metadata1.Disposable);
            metadata2.Uri.Should().Be(metadata1.Uri);
        }
    }
}
