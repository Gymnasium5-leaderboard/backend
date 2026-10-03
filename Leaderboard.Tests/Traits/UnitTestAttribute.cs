using Xunit.Sdk;

namespace Leaderboard.Tests.Traits;

[TraitDiscoverer("Leaderboard.Tests.Traits.UnitTestDiscoverer", "Leaderboard.Tests")]
[AttributeUsage(AttributeTargets.Class)]
public sealed class UnitTestAttribute : Attribute, ITraitAttribute;