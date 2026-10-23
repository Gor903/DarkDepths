using DarkDepths.Core;
using NUnit.Framework;

namespace DarkDepths.Tests.EditMode
{
    public sealed class ServicesTests
    {
        private interface IProbe
        {
            int Id { get; }
        }

        private sealed class Probe : IProbe
        {
            public Probe(int id)
            {
                Id = id;
            }

            public int Id { get; }
        }

        [SetUp]
        public void ClearBefore()
        {
            Services.Clear();
        }

        [TearDown]
        public void ClearAfter()
        {
            Services.Clear();
        }

        [Test]
        public void AServiceIsFoundByTheInterfaceItWasRegisteredAs()
        {
            var probe = new Probe(1);
            Services.Register<IProbe>(probe);

            Assert.AreSame(probe, Services.Get<IProbe>());
            Assert.IsTrue(Services.Has<IProbe>());
        }

        [Test]
        public void AskingForSomethingUnregisteredGivesNullRatherThanThrowing()
        {
            Assert.IsNull(Services.Get<IProbe>());
            Assert.IsFalse(Services.Has<IProbe>());
        }

        [Test]
        public void RegisteringAgainReplacesTheOldService()
        {
            Services.Register<IProbe>(new Probe(1));
            var second = new Probe(2);
            Services.Register<IProbe>(second);

            Assert.AreSame(second, Services.Get<IProbe>());
        }

        [Test]
        public void RegisteringNullTakesTheServiceAway()
        {
            Services.Register<IProbe>(new Probe(1));
            Services.Register<IProbe>(null);

            Assert.IsNull(Services.Get<IProbe>());
        }

        [Test]
        public void UnregisteringLeavesNothingBehind()
        {
            Services.Register<IProbe>(new Probe(1));
            Services.Unregister<IProbe>();

            Assert.IsFalse(Services.Has<IProbe>());
            Assert.IsNull(Services.Get<IProbe>());
        }

        [Test]
        public void UnregisteringWhatWasNeverThereIsHarmless()
        {
            Assert.DoesNotThrow(Services.Unregister<IProbe>);
        }
    }
}
