using Xunit;
using FluentAssertions;

namespace UriShell.Shell
{
	public class UriModuleItemResolverKeyTests
	{
        [Fact]
		public void EqualsIndependentOfCase()
		{
			var module = "Module";
			var item = "Item";

			var sampleKey = new UriModuleItemResolverKey(module, item);
			var upperKey = new UriModuleItemResolverKey(module.ToUpper(), item.ToUpper());
			var lowerKey = new UriModuleItemResolverKey(module.ToLower(), item.ToLower());

            sampleKey.Should().Be(upperKey);
			sampleKey.GetHashCode().Should().Be(upperKey.GetHashCode());

			sampleKey.Should().Be(lowerKey);
			sampleKey.GetHashCode().Should().Be(lowerKey.GetHashCode());

			upperKey.Should().Be(lowerKey);
			upperKey.GetHashCode().Should().Be(lowerKey.GetHashCode());
		}

        [Fact]
		public void DoesntEqualForDifferentModules()
		{
			var key1 = new UriModuleItemResolverKey("module1", "item");
			var key2 = new UriModuleItemResolverKey("module2", "item");

            key1.Should().NotBe(key2);
		}

        [Fact]
		public void DoesntEqualForDifferentItems()
		{
			var key1 = new UriModuleItemResolverKey("module", "item1");
			var key2 = new UriModuleItemResolverKey("module", "item2");

            key1.Should().NotBe(key2);
		}
	}
}
