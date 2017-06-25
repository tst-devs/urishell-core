using System;
using Xunit;
using FluentAssertions;

namespace UriShell.Shell
{
	public class ShellUriBuilderTest
	{
		public ShellUriBuilderTest()
		{
            UriShellSettings.Scheme = "tst";
		}

        [Fact]
        public void ParsesUriWhenCreatedWithUri()
        {
            var builder = new ShellUriBuilder(
                new Uri("tst://testplacement:102/testmodule/testitem"));

            builder.Placement.Should().Be("testplacement");
            builder.OwnerId.Should().Be(102);
            builder.Module.Should().Be("testmodule");
            builder.Item.Should().Be("testitem");
        }

        [Fact]
		public void ParsesUriParametersWhenCreatedWithUri()
		{
			var builder = new ShellUriBuilder(
                new Uri("tst://testplacement:102/testmodule/testitem?p1=test Param1&p2=test Param2"));

            builder.Parameters
                   .Should().HaveCount(2)
                   .And.Contain("p1", "test Param1")
                   .And.Contain("p2", "test Param2");
		}

		[Fact]
		public void BuildsUriWhenCreatedEmpty()
		{
            var builder = new ShellUriBuilder()
            {
                Placement = "newPlacement",
                OwnerId = 405,
                Module = "newModule",
                Item = "newItem"
            };

            builder.Uri.Should()
                   .Be(new Uri("tst://newPlacement:405/newModule/newItem"));
		}

		[Fact]
		public void BuildsUriWithParametersWhenCreatedEmpty()
		{
            var builder = new ShellUriBuilder()
            {
                Placement = "newPlacement",
                OwnerId = 405,
                Module = "newModule",
                Item = "newItem"
            };

            builder.Parameters["n1"] = "v10";
			builder.Parameters["n2"] = "v20";
			builder.Parameters["n3"] = "\t";

            builder.Uri.Should()
                   .Be(new Uri("tst://newPlacement:405/newModule/newItem?n1=v10&n2=v20&n3=%09"));
		}

		[Fact]
		public void BuildsUriFluently()
		{
			var uri = ShellUriBuilder
				.StartUri()
				.Placement("fluentPlacement")
				.OwnerId(134)
				.Module("fluentModule")
				.Item("fluentItem")
				.End();

            uri.Should()
               .Be(new Uri("tst://fluentPlacement:134/fluentModule/fluentItem"));
		}

        [Fact]
        public void BuildsUriWithParametersAndAttachmentsFluently()
        {
            var uri = ShellUriBuilder
                .StartUri()
                .Placement("fluentPlacement")
                .Module("fluentModule")
                .Item("fluentItem")
                .Parameter("p1", "v1")
                .Attachment("a1", 0)
                .Attachment("a2", 1)
                .End();

            uri.Should()
               .Be(new Uri("tst://fluentPlacement/fluentModule/fluentItem?p1=v1&a1={0}&a2={1}"));
        }
	}
}
