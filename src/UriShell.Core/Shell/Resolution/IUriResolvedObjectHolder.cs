using System;
using System.Collections.Generic;

namespace UriShell.Shell.Resolution
{
	public interface IUriResolvedObjectHolder : IEnumerable<object>
	{
		void Add(object resolved, UriResolvedMetadata metadata);

		void Remove(object resolved);

		bool Contains(object resolved);

		object Get(int id);

		UriResolvedMetadata GetMetadata(object resolved);
	}
}
