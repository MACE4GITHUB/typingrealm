using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Updating
{
    public class UpdateDetectorTests : TestsBase
    {
        [Theory, AutoMoqData]
        public void ShouldMarkGroupForUpdate(
            string[] groups,
            UpdateDetector sut)
        {
            foreach (var group in groups)
            {
                sut.MarkForUpdate(group);
            }

            var result = sut.PopMarked();
            foreach (var group in groups)
            {
                Assert.Contains(group, result);
            }
        }

        [Theory, AutoMoqData]
        public void ShouldNotMarkTheSameGroupTwice(
            string group,
            UpdateDetector sut)
        {
            sut.MarkForUpdate(group);
            sut.MarkForUpdate(group);

            Assert.Equal(group, sut.PopMarked().Single());
        }

        [Theory, AutoMoqData]
        public void ShouldPopAllMarkedGroups(UpdateDetector sut)
        {
            var groups = Fixture.CreateMany<string>(3).ToList();
            foreach (var group in groups)
            {
                sut.MarkForUpdate(group);
            }

            Assert.Equal(3, sut.PopMarked().Count());
            Assert.Empty(sut.PopMarked());

            sut.MarkForUpdate(groups[0]);
            Assert.Equal(groups[0], sut.PopMarked().Single());
            Assert.Empty(sut.PopMarked());
        }

        // This test is likely useless and locking is not properly tested.
        [Theory, AutoMoqData]
        public async Task ShouldMarkThreadSafely(UpdateDetector sut)
        {
            var tasks = new List<Task>(100);
            for (var i = 1; i <= 100; i++)
            {
                var group = Create<string>();
                tasks.Add(new Task(() => sut.MarkForUpdate(group)));
                tasks.Add(new Task(() => sut.MarkForUpdate(group)));
            }

            Parallel.ForEach(tasks, t => t.Start());
            await Task.WhenAll(tasks);

            Assert.Equal(100, sut.PopMarked().Count());
        }
    }
}
