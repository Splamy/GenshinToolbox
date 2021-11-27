namespace GenshinToolbox.ArtScraper;

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

	private static readonly Dictionary<ArtSet, string> SetNames = new()
	{
		{ ArtSet.GladiatorsFinale, "Gladiator's Finale" },
		{ ArtSet.WanderersTroupe, "Wanderer's Troupe" },
		{ ArtSet.ViridescentVenerer, "Viridescent Venerer" },
		{ ArtSet.ThunderingFury, "Thundering Fury" },
		{ ArtSet.Thundersoother, "Thundersoother" },
		{ ArtSet.CrimsonWitchOfFlames, "Crimson Witch of Flames" },
		{ ArtSet.Lavawalker, "Lavawalker" },
		{ ArtSet.ArchaicPetra, "Archaic Petra" },
		{ ArtSet.RetracingBolide, "Retracing Bolide" },
		{ ArtSet.MaidenBeloved, "Maiden Beloved" },
		{ ArtSet.NoblesseOblige, "Noblesse Oblige" },
		{ ArtSet.BloodstainedChivalry, "Bloodstained Chivalry" },
		{ ArtSet.BlizzardStrayer, "Blizzard Strayer" },
		{ ArtSet.HeartOfDepth, "Heart of Depth" },
		{ ArtSet.TenacityOfTheMillelith, "Tenacity of the Millelith" },
		{ ArtSet.PaleFlame, "Pale Flame" },
		{ ArtSet.EmblemOfSeveredFate, "Emblem of Severed Fate" },
		{ ArtSet.ShimenawasReminiscence, "Shimenawa's Reminiscence" },
		{ ArtSet.Instructor, "Instructor" },
		{ ArtSet.TheExile, "The Exile" },
		{ ArtSet.ResolutionOfSojourner, "Resolution of Sojourner" },
		{ ArtSet.MartialArtist, "Martial Artist" },
		{ ArtSet.DefendersWill, "Defender's Will" },
		{ ArtSet.TinyMiracle, "Tiny Miracle" },
		{ ArtSet.BraveHeart, "Brave Heart" },
		{ ArtSet.Gambler, "Gambler" },
		{ ArtSet.Scholar, "Scholar" },
		{ ArtSet.PrayersForWisdom, "Prayers for Wisdom" },
		{ ArtSet.PrayersToSpringtime, "Prayers to Springtime" },
		{ ArtSet.PrayersForIllumination, "Prayers for Illumination" },
		{ ArtSet.PrayersForDestiny, "Prayers for Destiny" },
		{ ArtSet.Berserker, "Berserker" },
		{ ArtSet.LuckyDog, "Lucky Dog" },
		{ ArtSet.TravelingDoctor, "Traveling Doctor" },
		{ ArtSet.Adventurer, "Adventurer" },
		{ ArtSet.Initiate, "Initiate" },
	};

	private static readonly Dictionary<Slot, string> SlotNames = new()
	{
		{ Slot.Flower, "Flower of Life" },
		{ Slot.Plume, "Plume of Death" },
		{ Slot.Sands, "Sands of Eon" },
		{ Slot.Goblet, "Goblet of Eonothem" },
		{ Slot.Circlet, "Circlet of Logos" },
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

	record struct SlotStatsGroup(IReadOnlyList<Stat> Main, IReadOnlyList<Stat> Sub);

	private static readonly Dictionary<Slot, SlotStatsGroup> StatCategory = new()
	{
		{ Slot.Flower, new(new[] { Stat.Hp }, SubStats) },
		{ Slot.Plume, new(new[] { Stat.Atk }, SubStats) },
		{ Slot.Sands, new(new[] { Stat.HpPerc, Stat.AtkPerc, Stat.DefPerc, Stat.ElementalMastery, Stat.EnergyRecharge }, SubStats) },
		{ Slot.Goblet, new(new[] { Stat.HpPerc, Stat.AtkPerc, Stat.DefPerc, Stat.ElementalMastery, Stat.PyroDmgBonus, Stat.HydroDmgBonus, Stat.ElectroDmgBonus, Stat.AnemoDmgBonus, Stat.CryoDmgBonus, Stat.GeoDmgBonus, Stat.PhysicalDmgBonus, }, SubStats) },
		{ Slot.Circlet, new(new[] { Stat.HpPerc, Stat.AtkPerc, Stat.DefPerc, Stat.ElementalMastery, Stat.CritRate, Stat.CritDmg, Stat.HealingBonusPerc }, SubStats) },
	};
}
