using System;
using System.Collections;
using System.Collections.Generic;

namespace UriShell.Shell.Resolution
{
    public sealed class UriResolvedObjectHolder : IUriResolvedObjectHolder
    {
        public const int MaxResolvedId = 2 << 12;

        private readonly Queue<int> _idPool;

        private readonly Dictionary<object, UriResolvedMetadata> _byResolved = new Dictionary<object, UriResolvedMetadata>();

        private readonly Dictionary<int, object> _byId = new Dictionary<int, object>();

        public UriResolvedObjectHolder()
        {
            var ids = new int[MaxResolvedId + 1];
            for (var i = 0; i < ids.Length; i++)
            {
                ids[i] = i;
            }

            _idPool = new Queue<int>(ids.Length);

            var random = new Random();
            for (var i = ids.Length; i > 0; i--)
            {
                var index = random.Next(i);
                var id = ids[index];
                ids[index] = ids[i - 1];
                ids[i - 1] = id;

                _idPool.Enqueue(id);
            }
        }

        public void Add(object resolved, UriResolvedMetadata metadata)
        {
            if (resolved == null)
            {
                throw new ArgumentNullException(nameof(resolved));
            }
            if (metadata.Uri == null)
            {
                throw new ArgumentNullException(nameof(metadata.Uri));
            }
            if (metadata.Disposable == null)
            {
                throw new ArgumentNullException(nameof(metadata.Disposable));
            }
            if (_idPool.Count == 0)
            {
                throw new InvalidOperationException("Amount of URI, that could be opened, is exceeded.");
            }

            var id = _idPool.Dequeue();
            try
            {
                _byResolved.Add(resolved, metadata.AssignId(id));
            }
            catch (ArgumentException)
            {
                throw new ArgumentException(
                    $"Object {resolved} already exists in {nameof(IUriResolvedObjectHolder)}.",
                    nameof(resolved));
            }

            _byId.Add(id, resolved);
        }

        public void Remove(object resolved)
        {
            if (resolved == null)
            {
                throw new ArgumentNullException(nameof(resolved));
            }

            if (_byResolved.TryGetValue(resolved, out var metadata))
            {
                _byResolved.Remove(resolved);
                _byId.Remove(metadata.ResolvedId);
                _idPool.Enqueue(metadata.ResolvedId);
            }
        }

        public bool Contains(object resolved)
        {
            if (resolved == null)
            {
                return false;
            }

            return _byResolved.ContainsKey(resolved);
        }

        public object Get(int id)
        {
            if (_byId.TryGetValue(id, out var resolved))
            {
                return resolved;
            }

            throw new ArgumentOutOfRangeException(
                nameof(id),
                $"Object with ID {id} doesn't exist in {nameof(IUriResolvedObjectHolder)}.");
        }

        public UriResolvedMetadata GetMetadata(object resolved)
        {
            if (resolved == null)
            {
                throw new ArgumentNullException(nameof(resolved));
            }

            if (_byResolved.TryGetValue(resolved, out var metadata))
            {
                return metadata;
            }

            throw new ArgumentOutOfRangeException(
                nameof(resolved),
                $"Object {resolved} doesn't exist in {nameof(IUriResolvedObjectHolder)}.");
        }

        public IEnumerator<object> GetEnumerator() => _byResolved.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}