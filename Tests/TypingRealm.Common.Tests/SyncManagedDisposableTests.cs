using System;
using System.Threading.Tasks;
using Xunit;

namespace TypingRealm.Tests
{
    public class TestSyncManagedDisposable : SyncManagedDisposable
    {
        public bool IsDisposed { get; set; }

        protected override void DisposeManagedResources()
        {
            IsDisposed = true;
        }
    }

    public class SyncManagedDisposableTests : IDisposable
    {
        private readonly TestSyncManagedDisposable _sut;

        public SyncManagedDisposableTests()
        {
            _sut = new TestSyncManagedDisposable();
        }

        public void Dispose()
        {
            _sut.Dispose();
        }

        [Fact]
        public void ShouldInheritManagedDisposable()
        {
            Assert.IsAssignableFrom<ManagedDisposable>(_sut);
        }

        [Fact]
        public void ShouldDisposeSync()
        {
            _sut.Dispose();
            Assert.True(_sut.IsDisposed);
        }

        [Fact]
        public async Task ShouldDisposeAsync()
        {
            await _sut.DisposeAsync();
            Assert.True(_sut.IsDisposed);
        }
    }
}
