namespace GenshinToolbox.ArtScraper;

enum Stat
{
	Atk,
	AtkPerc,
	Hp,
	HpPerc,
	Def,
	DefPerc,
	EnergyRecharge,
	ElementalMastery,
	CritDmg,
	CritRate,

	PyroDmgBonus,
	HydroDmgBonus,
	ElectroDmgBonus,
	AnemoDmgBonus,
	CryoDmgBonus,
	GeoDmgBonus,
	PhysicalDmgBonus,
	HealingBonusPerc,
}

partial class Scraper
{
	private static readonly Dictionary<Stat, string> StatNames = new()
	{
		{ Stat.Atk, "ATK" },
		{ Stat.AtkPerc, "ATK" },
		{ Stat.Hp, "HP" },
		{ Stat.HpPerc, "HP" },
		{ Stat.Def, "DEF" },
		{ Stat.DefPerc, "DEF" },
		{ Stat.EnergyRecharge, "Energy Recharge" },
		{ Stat.ElementalMastery, "Elemental Mastery" },
		{ Stat.CritDmg, "CRIT DMG" },
		{ Stat.CritRate, "CRIT Rate" },
		{ Stat.PyroDmgBonus, "Pyro DMG Bonus" },
		{ Stat.HydroDmgBonus, "Hydro DMG Bonus" },
		{ Stat.ElectroDmgBonus, "Electro DMG Bonus" },
		{ Stat.AnemoDmgBonus, "Anemo DMG Bonus" },
		{ Stat.CryoDmgBonus, "Cryo DMG Bonus" },
		{ Stat.GeoDmgBonus, "Geo DMG Bonus" },
		{ Stat.PhysicalDmgBonus, "Physical DMG Bonus" },
		{ Stat.HealingBonusPerc, "Healing Bonus" },
	};

	private static readonly Stat[] SubStats = new[] {
		Stat.Atk,
		Stat.AtkPerc,
		Stat.Hp,
		Stat.HpPerc,
		Stat.Def,
		Stat.DefPerc,
		Stat.EnergyRecharge,
		Stat.ElementalMastery,
		Stat.CritDmg,
		Stat.CritRate,
	};
}