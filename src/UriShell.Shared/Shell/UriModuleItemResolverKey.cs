using System;

namespace UriShell.Shell
{
    public sealed class UriModuleItemResolverKey : IEquatable<UriModuleItemResolverKey>
    {
        public UriModuleItemResolverKey(string module, string item)
        {
            Module = module?.ToLower();
            Item = item?.ToLower();
        }

        public string Module { get; private set; }

        public string Item { get; private set; }

        public override bool Equals(object obj)
        {
			if (obj != null && obj.GetType() == GetType())
            {
                return Equals((UriModuleItemResolverKey)obj);
            }

            return false;
        }

        public bool Equals(UriModuleItemResolverKey other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

			return Module == other.Module && Item == other.Item;
        }

        public override int GetHashCode()
        {
            return Module.GetHashCode() ^ Item.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Module}/{Item}";
        }
    }
}
