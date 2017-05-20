using System;
using System.Collections.Generic;
using System.Linq;

namespace UriShell.Shell
{
	public static class ShellExtensions
	{
		public static void CloseResolvedList(this IShell shell, IEnumerable<object> resolvedList)
		{
            if (shell == null)
            {
                throw new ArgumentNullException(nameof(shell));
            }
            if (resolvedList == null)
			{
				throw new ArgumentNullException(nameof(resolvedList));
			}

            resolvedList.ToList().ForEach(shell.CloseResolved);
		}
	}
}
