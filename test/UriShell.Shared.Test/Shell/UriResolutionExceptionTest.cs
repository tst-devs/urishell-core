using System;
using Xunit;
using FluentAssertions;

namespace UriShell.Shell
{
	public class UriResolutionExceptionTests
	{
		[Fact]
		public void IncludesUriInMessage()
		{
			var uri = new Uri("http://web.com");
			var exception = new UriResolutionException(uri, "test message");

            exception.Message.Should().Contain(uri.ToString());
		}
	}
}
