namespace GenshinToolbox.ArtScraper;

enum Slot
{
	Flower,
	Plume,
	Sands,
	Goblet,
	Circlet
}

partial class Scraper
{
	private static readonly Dictionary<Slot, string> SlotNames = new()
	{
		{ Slot.Flower, "Flower of Life" },
		{ Slot.Plume, "Plume of Death" },
		{ Slot.Sands, "Sands of Eon" },
		{ Slot.Goblet, "Goblet of Eonothem" },
		{ Slot.Circlet, "Circlet of Logos" },
	};

	private static readonly Dictionary<Slot, (Stat[] main, Stat[] sub)> StatCategory = new()
	{
		{ Slot.Flower, (new[] { Stat.Hp }, SubStats) },
		{ Slot.Plume, (new[] { Stat.Atk }, SubStats) },
		{ Slot.Sands, (new[] { Stat.HpPerc, Stat.AtkPerc, Stat.DefPerc, Stat.ElementalMastery, Stat.EnergyRecharge }, SubStats) },
		{ Slot.Goblet, (new[] { Stat.HpPerc, Stat.AtkPerc, Stat.DefPerc, Stat.ElementalMastery, Stat.PyroDmgBonus, Stat.HydroDmgBonus, Stat.ElectroDmgBonus, Stat.AnemoDmgBonus, Stat.CryoDmgBonus, Stat.GeoDmgBonus, Stat.PhysicalDmgBonus, }, SubStats) },
		{ Slot.Circlet, (new[] { Stat.HpPerc, Stat.AtkPerc, Stat.DefPerc, Stat.ElementalMastery, Stat.CritRate, Stat.CritDmg, Stat.HealingBonusPerc }, SubStats) },
	};
}