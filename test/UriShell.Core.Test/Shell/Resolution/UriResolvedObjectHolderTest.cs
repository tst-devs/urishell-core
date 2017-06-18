using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

using NSubstitute;
using Xunit;

namespace UriShell.Shell.Resolution
{
    public class UriResolvedObjectHolderTest
    {
        private const int GenerationTimeout = 300;

        private readonly UriResolvedMetadata _uriResolvedMetadata =
            new UriResolvedMetadata(new Uri("tst://p/m/v"), Substitute.For<IDisposable>());

        [Fact]
        public void AddsObjectsToHolder()
        {
            var object1 = new object();
            var object2 = new object();

            var holder = new UriResolvedObjectHolder
            {
                { object1, _uriResolvedMetadata },
                { object2, _uriResolvedMetadata }
            };

            holder.Should()
                  .HaveCount(2)
                  .And.Contain(object1)
                  .And.Contain(object2);
        }

        [Fact]
        public void AddsObjectsMetadataToHolder()
        {
            var object1 = new object();
            var object2 = new object();

            var metadata1 = new UriResolvedMetadata(new Uri("tst://p1/md1/vw1"), Substitute.For<IDisposable>());
            var metadata2 = new UriResolvedMetadata(new Uri("tst://p2/md2/vw2"), Substitute.For<IDisposable>());

            var holder = new UriResolvedObjectHolder
            {
                { object1, metadata1 },
                { object2, metadata2 }
            };

            var holderMetadata1 = holder.GetMetadata(object1);
            var holderMetadata2 = holder.GetMetadata(object2);

            // Ids are assinged by holder, so we need to change metadata here.
            // It is ok for test, because we don't test id assignment.
            metadata1 = metadata1.AssignId(holderMetadata1.ResolvedId);
            metadata2 = metadata2.AssignId(holderMetadata2.ResolvedId);

            holderMetadata1.Should().Be(metadata1);
            holderMetadata2.Should().Be(metadata2);
        }

        [Fact]
        public void RaisesExceptionWhenAddingTheSameObjectTwice()
        {
            var object1 = new object();

            var holder = new UriResolvedObjectHolder
            {
                { object1, _uriResolvedMetadata }
            };

            holder.Invoking(h => h.Add(object1, _uriResolvedMetadata))
                  .ShouldThrow<ArgumentException>()
                  .And.ParamName.Should().Be("resolved");
        }

        [Fact]
        public void RemovesObjectFromHolder()
        {
            var object1 = new object();
            var object2 = new object();

            var holder = new UriResolvedObjectHolder
            {
                { object1, _uriResolvedMetadata },
                { object2, _uriResolvedMetadata }
            };

            holder.Remove(object1);

            holder.Should().NotContain(object1);
        }

        [Fact]
        public void DoesntRaiseExceptionWhenRemovingTheSameObjectTwice()
        {
            var object1 = new object();

            var holder = new UriResolvedObjectHolder
            {
                { object1, _uriResolvedMetadata }
            };

            holder.Remove(object1);

            holder.Invoking(h => h.Remove(object1))
                  .ShouldNotThrow();
        }

        [Fact]
        public void GeneratesUniqueIds()
        {
            var objects = Enumerable
                .Range(0, UriResolvedObjectHolder.MaxResolvedId + 1)
                .Select(_ => new object())
                .ToList();

			Task.Run(() =>
	            {
	                var ids = new HashSet<int>();
	                var holder = new UriResolvedObjectHolder();

	                foreach (var obj in objects)
	                {
	                    holder.Add(obj, _uriResolvedMetadata);

	                    var id = holder.GetMetadata(obj).ResolvedId;
	                    ids.Add(id).Should().BeTrue();
	                }
	            })
	            .Wait(GenerationTimeout).Should().BeTrue();
        }

        [Fact]
        public void RaisesExceptionWhenIdsExceedViewIdContraints()
        {
            var objects = Enumerable
                .Range(0, UriResolvedObjectHolder.MaxResolvedId + 1)
                .Select(_ => new object())
                .ToList();

			Task.Run(() =>
	            {
	                var holder = new UriResolvedObjectHolder();

	                foreach (var obj in objects)
	                {
	                    holder.Add(obj, _uriResolvedMetadata);
	                }

	                holder.Invoking(h => h.Add(new object(), _uriResolvedMetadata))
	                      .ShouldThrow<InvalidOperationException>();
	            })
                .Wait(GenerationTimeout).Should().BeTrue();
        }

        [Fact]
        public void ReuseIdAfterObjectRemoving()
        {
			var object1 = new object();
			var object2 = new object();

			var objects = Enumerable
                .Range(0, UriResolvedObjectHolder.MaxResolvedId - 1)
				.Select(_ => new object())
				.Concat(new object[] { object1, object2 })
                .ToList();

			Task.Run(() =>
                {
                    var holder = new UriResolvedObjectHolder();

                    foreach (var obj in objects)
                    {
                        holder.Add(obj, _uriResolvedMetadata);
                    }

                    var id1 = holder.GetMetadata(object1).ResolvedId;
                    var id2 = holder.GetMetadata(object2).ResolvedId;

                    holder.Remove(object1);
                    holder.Remove(object2);

                    var object3 = new object();
                    holder.Add(object3, _uriResolvedMetadata);

                    var id3 = holder.GetMetadata(object3).ResolvedId;
                    (id3 == id1 || id3 == id2).Should().BeTrue();

                    var object4 = new object();
                    holder.Add(object4, _uriResolvedMetadata);

                    var id4 = holder.GetMetadata(object4).ResolvedId;
                    id4.Should().NotBe(id3);
                    (id4 == id1 || id4 == id2).Should().BeTrue();
                })
                .Wait(GenerationTimeout).Should().BeTrue();
        }

        [Fact]
        public void ContainsReturnsTrueForAddedObjects()
        {
            var object1 = new object();
            var object2 = new object();

            var holder = new UriResolvedObjectHolder
            {
                { object1, _uriResolvedMetadata },
                { object2, _uriResolvedMetadata }
            };

            holder.Contains(object1).Should().BeTrue();
            holder.Contains(object2).Should().BeTrue();
        }

        [Fact]
        public void ContainsReturnsFalseForRemovedObjects()
        {
            var object1 = new object();
            var object2 = new object();

            var holder = new UriResolvedObjectHolder();
            holder.Add(object1, _uriResolvedMetadata);
            holder.Remove(object1);

            holder.Contains(object1).Should().BeFalse();
        }

        [Fact]
        public void ContainsReturnsFalseForUnknownObjectsAndNull()
        {
            var holder = new UriResolvedObjectHolder();

            holder.Contains(new object()).Should().BeFalse();
            holder.Contains(null).Should().BeFalse();
        }

        [Fact]
        public void GetsObjectsById()
        {
            var object1 = new object();
            var object2 = new object();

            var holder = new UriResolvedObjectHolder
            {
                { object1, _uriResolvedMetadata },
                { object2, _uriResolvedMetadata }
            };

            var id1 = holder.GetMetadata(object1).ResolvedId;
            var id2 = holder.GetMetadata(object2).ResolvedId;

            holder.Get(id1).Should().Be(object1);
            holder.Get(id2).Should().Be(object2);
        }

        [Fact]
        public void RaisesExceptionWhenGettingObjectByUnknownId()
        {
            var holder = new UriResolvedObjectHolder();

            holder.Invoking(h => h.Get(9901))
                  .ShouldThrow<ArgumentOutOfRangeException>()
                  .And.ParamName.Should().Be("id");
        }

        [Fact]
        public void RaisesExceptionWhenGettingMetadataForUnknownObject()
        {
            var holder = new UriResolvedObjectHolder();

            holder.Invoking(h => h.GetMetadata(new object()))
                  .ShouldThrow<ArgumentOutOfRangeException>()
                  .And.ParamName.Should().Be("resolved");
        }
    }
}
