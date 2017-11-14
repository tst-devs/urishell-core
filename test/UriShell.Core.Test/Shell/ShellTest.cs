using System;
using FluentAssertions;

using Xunit;

using NSubstitute;

using UriShell.Shell.Registration;
using UriShell.Shell.Resolution;

namespace UriShell.Shell
{
    using ShellResolveFactory = Func<Uri, object[], IShellResolve>;

    public class ShellTest
    {
        private IUriResolvedObjectHolder _uriResolvedObjectHolder =
            Substitute.For<IUriResolvedObjectHolder>();

        public ShellTest()
        {
            UriShellSettings.Scheme = "tst";
        }

        private Shell CreateShell(
            ShellResolveFactory shellResolveFactory = null)
        {
            return new Shell(
                _uriResolvedObjectHolder,
                shellResolveFactory ?? Substitute.For<ShellResolveFactory>());
        }

        [Fact]
        public void ResolvesUriUsingShellResolveFactory()
        {
            Uri factoryUri = null;
            object[] factoryAttachments = null;

            var factory = new ShellResolveFactory(
                (uri, attachments) =>
                {
                    factoryUri = uri;
                    factoryAttachments = attachments;

                    return Substitute.For<IShellResolve>();
                });

            var shell = CreateShell(shellResolveFactory: factory);
            shell.Resolve(new Uri("tst://tab/module/item"), 1, "2");

            factoryUri.OriginalString.Should().Be("tst://tab/module/item");
            factoryAttachments.Should()
                              .HaveCount(2)
                              .And.Contain(1)
                              .And.Contain("2");
        }

        [Fact]
        public void ReturnsResolvedIsOpenWhenContainedInUriResolvedObjectHolder()
        {
            var resolved = new object();

            var shell = CreateShell();
            shell.IsResolvedOpen(resolved);

            _uriResolvedObjectHolder.Received(1).Contains(resolved);
        }

        [Fact]
        public void GetsResolvedIdFormUriResolvedObjectHolder()
        {
            var resolved = new object();
            var metadata = new UriResolvedMetadata(null, null).AssignId(1334);

            _uriResolvedObjectHolder.Contains(resolved).Returns(true);
            _uriResolvedObjectHolder.GetMetadata(resolved).Returns(metadata);

            var shell = CreateShell();
            var id = shell.GetResolvedId(resolved);

            id.Should().Be(metadata.ResolvedId);
        }

        [Fact]
        public void GetsResolvedUriFormUriResolvedObjectHolder()
        {
            var resolved = new object();
            var metadata = new UriResolvedMetadata(new Uri("tst://tab/module/item"), null);

            _uriResolvedObjectHolder.Contains(resolved).Returns(true);
            _uriResolvedObjectHolder.GetMetadata(resolved).Returns(metadata);

            var shell = CreateShell();
            var uri = shell.GetResolvedUri(resolved);

            uri.Should().Be(metadata.Uri);
        }

        [Fact]
        public void ClosesResolvedUsingMetadataDisposable()
        {
            var resolved = new object();
            var disposable = Substitute.For<IDisposable>();
            var metadata = new UriResolvedMetadata(new Uri("tst://tab/module/item"), disposable);

            _uriResolvedObjectHolder.Contains(resolved).Returns(true);
            _uriResolvedObjectHolder.GetMetadata(resolved).Returns(metadata);

            disposable
                .When(d => d.Dispose())
                .Do(_ => _uriResolvedObjectHolder.Contains(resolved).Returns(false));

            var shell = CreateShell();
            shell.CloseResolved(resolved);

            disposable.Received(1).Dispose();
        }

        [Fact]
        public void ExposesUriModuleItemResolversInUriResolutionCustomization()
        {
            var shell = CreateShell();
            var expectedResolver = Substitute.For<IUriModuleItemResolver>();
            shell.AddUriModuleItemResolver(new UriModuleItemResolverKey("module", "item"), expectedResolver);

            shell.ModuleItemResolvers.TryGetValue(
                new UriModuleItemResolverKey("othermodule", "otheritem"),
                out var actualResolver1);
            actualResolver1.Should().BeNull();

            shell.ModuleItemResolvers.TryGetValue(
                new UriModuleItemResolverKey("module", "item"),
                out var actualResolver2);
            actualResolver2.Should().Be(expectedResolver);
        }

        [Fact]
        public void ExposesUriPlacementResolversInUriResolutionCustomization()
        {
            var uriPlacementResolver1 = Substitute.For<IUriPlacementResolver>();
            var uriPlacementResolver2 = Substitute.For<IUriPlacementResolver>();

            var shell = CreateShell();
            shell.AddUriPlacementResolver(uriPlacementResolver1);
            shell.AddUriPlacementResolver(uriPlacementResolver2);

            shell.PlacementResolvers.Should()
                 .HaveCount(2)
                 .And.Contain(uriPlacementResolver1)
                 .And.Contain(uriPlacementResolver2);
        }

        [Fact]
        public void DoesntAddTheSameUriPlacementResolverTwice()
        {
            var uriPlacementResolver = Substitute.For<IUriPlacementResolver>();

            var shell = CreateShell();
            shell.AddUriPlacementResolver(uriPlacementResolver);
            shell.AddUriPlacementResolver(uriPlacementResolver);

            shell.PlacementResolvers.Should().HaveCount(1);
        }

        [Fact]
        public void DoesntHoldStrongReferenceToUriPlacementResolvers()
        {
            var uriPlacementResolver1 = Substitute.For<IUriPlacementResolver>();
            var uriPlacementResolver2 = Substitute.For<IUriPlacementResolver>();
            var uriPlacementResolver3 = Substitute.For<IUriPlacementResolver>();

            var shell = CreateShell();
            shell.AddUriPlacementResolver(uriPlacementResolver1);
            shell.AddUriPlacementResolver(uriPlacementResolver2);
            shell.AddUriPlacementResolver(uriPlacementResolver3);

            shell.PlacementResolvers.Should().HaveCount(3);

            uriPlacementResolver1 = null;
            uriPlacementResolver2 = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            shell.Resolve(new Uri("tst://tab/module/item"));

            shell.PlacementResolvers.Should().HaveCount(1);

            GC.KeepAlive(uriPlacementResolver3);
        }

        [Fact]
        public void ReturnsNullWhenTryingToParseNonHyperlink()
        {
            var shell = CreateShell();
            var source = "hello world";

            shell.TryParseHyperlink(source, 1).Should().BeNull();
        }

        [Fact]
        public void ReturnsNullWhenTryingToParseHyperlinkWithoutHref()
        {
            var shell = CreateShell();
            var source = "<a>hello world</a>";

            shell.TryParseHyperlink(source, 1).Should().BeNull();
        }

        [Fact]
        public void ReturnsNullWhenTryingToParseParsesHyperlinkWithSingleQuotes()
        {
            var shell = CreateShell();
            var source = "<a href='tst://tab/web/browser'>hello world</a>";

            shell.TryParseHyperlink(source, 1).Should().BeNull();
        }

        [Fact]
        public void OmitsOwnerIdWhenTryingToParseNonShellHyperlink()
        {
            var shell = CreateShell();
            var source = string.Format("<a href=\"http://ya.ru/logo.png\">hello world</a>");

            var hyperlink = shell.TryParseHyperlink(source, 3);

            hyperlink.Uri.Should().Be(new Uri("http://ya.ru/logo.png"));
        }

        [Fact]
        public void ParsesHyperlinkUriAndTextFromValidHypelink()
        {
            var shell = CreateShell();
            var source = string.Format("<a href=\"tst://tab/web/browser\">hello world</a>");

            var hyperlink = shell.TryParseHyperlink(source, 3);

            hyperlink.Text.Should().Be("hello world");
            hyperlink.Uri.Should().Be(new Uri("tst://tab:3/web/browser"));
        }

        [Fact]
        public void ParsesHyperlinkWithEmptyTitle()
        {
            var shell = CreateShell();
            var source = string.Format("<a href=\"tst://tab/web/browser\"></a>");

            shell.TryParseHyperlink(source, 1).Should().NotBeNull();
        }

        [Fact]
        public void CreatesHyperlinkFromUri()
        {
            var uri = new Uri(string.Format(
                "tst://placement:1111/module/item?&title={0}&icon={1}",
                Uri.EscapeDataString("Test Title"),
                Uri.EscapeDataString("http://ya.ru/logo.png")));

            var shell = CreateShell();
            var hyperlink = shell.CreateHyperlink(uri);

            hyperlink.Text.Should().Be("Test Title");
            hyperlink.Uri.Should().Be(uri);
            hyperlink.Icon.Should().Be(new Uri("http://ya.ru/logo.png"));
        }

        //
        // [Mx] The test is not valid at that moment, because "/" is valid
        // starting point of absolute URI in Unix. For details:
        // https://github.com/dotnet/corefx/issues/22098
        // 
        //[Fact]
        //public void ThrowsWhenCreatesHyperlinkFromUriWithRelativeIcon()
        //{
        //    var uri = new Uri(string.Format(
        //        "tst://placement:1111/module/item?title={0}&icon={1}",
        //        Uri.EscapeDataString("Test Title"),
        //        Uri.EscapeDataString("/images/logo.png")));
        //
        //    var shell = CreateShell();
        //
        //    shell.Invoking(s => s.CreateHyperlink(uri))
        //         .ShouldThrow<ArgumentException>();
        //}

        [Fact]
        public void UpdatesResolvedUri()
        {
            var resolved = new object();
            var metadata = new UriResolvedMetadata(new Uri("tst://tab/module/item"), null);

            _uriResolvedObjectHolder.Contains(resolved).Returns(true);
            _uriResolvedObjectHolder.GetMetadata(resolved).Returns(metadata);

            var shell = CreateShell();
            var newUri = new Uri("tst://tab/module/item?param=1");
            shell.UpdateResolvedUri(resolved, newUri);

            shell.GetResolvedUri(resolved).Should().Be(newUri);
        }
    }
}
