using System.Collections.Generic;

namespace DungeonSurvivors.Rules
{
    /// <summary>A weapon whose numbers the level-up choice can raise. The Run asks each weapon what it offers and hands a pick to the weapon it belongs to.</summary>
    internal interface IUpgradeable
    {
        /// <summary>Every upgrade this weapon offers, maxed or not.</summary>
        IReadOnlyList<Upgrade> AllUpgrades { get; }

        /// <summary>Raises the number <paramref name="upgrade"/> belongs to by one level. The caller checks it is offered.</summary>
        void Apply(Upgrade upgrade);

        /// <summary>Adds the upgrades this weapon offers right now to <paramref name="pool"/>.</summary>
        void AddAvailableUpgrades(List<Upgrade> pool);
    }
}
