using Xunit.Sdk;

namespace Leaderboard.Tests.Traits;

[TraitDiscoverer("Leaderboard.Tests.Traits.FunctionalTestDiscoverer", "Leaderboard.Tests")]
[AttributeUsage(AttributeTargets.Class)]
public sealed class FunctionalTestAttribute : Attribute, ITraitAttribute;