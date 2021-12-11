using System.Collections.Generic;

namespace GenshinToolbox.ArtScraper;

partial class Scraper
{
	private static readonly IReadOnlyDictionary<Stat, string> StatNames = new Dictionary<Stat, string>()
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

	private static readonly IReadOnlyDictionary<ArtSet, string> SetNames = new Dictionary<ArtSet, string>()
	{
		{ ArtSet.Adventurer, "Adventurer" },
		{ ArtSet.ArchaicPetra, "Archaic Petra" },
		{ ArtSet.Berserker, "Berserker" },
		{ ArtSet.BlizzardStrayer, "Blizzard Strayer" },
		{ ArtSet.BloodstainedChivalry, "Bloodstained Chivalry" },
		{ ArtSet.BraveHeart, "Brave Heart" },
		{ ArtSet.CrimsonWitchOfFlames, "Crimson Witch of Flames" },
		{ ArtSet.DefendersWill, "Defender's Will" },
		{ ArtSet.EmblemOfSeveredFate, "Emblem of Severed Fate" },
		{ ArtSet.Gambler, "Gambler" },
		{ ArtSet.GladiatorsFinale, "Gladiator's Finale" },
		{ ArtSet.HeartOfDepth, "Heart of Depth" },
		{ ArtSet.HuskOfOpulentDreams, "Husk of Opulent Dreams" },
		{ ArtSet.Initiate, "Initiate" },
		{ ArtSet.Instructor, "Instructor" },
		{ ArtSet.Lavawalker, "Lavawalker" },
		{ ArtSet.LuckyDog, "Lucky Dog" },
		{ ArtSet.MaidenBeloved, "Maiden Beloved" },
		{ ArtSet.MartialArtist, "Martial Artist" },
		{ ArtSet.NoblesseOblige, "Noblesse Oblige" },
		{ ArtSet.OceanHuedClam, "Ocean-Hued Clam" },
		{ ArtSet.PaleFlame, "Pale Flame" },
		{ ArtSet.PrayersForDestiny, "Prayers for Destiny" },
		{ ArtSet.PrayersForIllumination, "Prayers for Illumination" },
		{ ArtSet.PrayersForWisdom, "Prayers for Wisdom" },
		{ ArtSet.PrayersToSpringtime, "Prayers to Springtime" },
		{ ArtSet.ResolutionOfSojourner, "Resolution of Sojourner" },
		{ ArtSet.RetracingBolide, "Retracing Bolide" },
		{ ArtSet.Scholar, "Scholar" },
		{ ArtSet.ShimenawasReminiscence, "Shimenawa's Reminiscence" },
		{ ArtSet.TenacityOfTheMillelith, "Tenacity of the Millelith" },
		{ ArtSet.TheExile, "The Exile" },
		{ ArtSet.ThunderingFury, "Thundering Fury" },
		{ ArtSet.Thundersoother, "Thundersoother" },
		{ ArtSet.TinyMiracle, "Tiny Miracle" },
		{ ArtSet.TravelingDoctor, "Traveling Doctor" },
		{ ArtSet.ViridescentVenerer, "Viridescent Venerer" },
		{ ArtSet.WanderersTroupe, "Wanderer's Troupe" },
	};

	private static readonly IReadOnlySet<ArtSet> UnusableSets = new HashSet<ArtSet> { ArtSet.Initiate };

	private static readonly IReadOnlyDictionary<Slot, string> SlotNames = new Dictionary<Slot, string>()
	{
		{ Slot.Flower, "Flower of Life" },
		{ Slot.Plume, "Plume of Death" },
		{ Slot.Sands, "Sands of Eon" },
		{ Slot.Goblet, "Goblet of Eonothem" },
		{ Slot.Circlet, "Circlet of Logos" },
	};

	private static readonly IReadOnlySet<Stat> SubStats = new HashSet<Stat> {
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

	record struct SlotStatsGroup(IReadOnlySet<Stat> Main, IReadOnlySet<Stat> Sub);

	private static readonly Dictionary<Slot, SlotStatsGroup> StatCategory = new()
	{
		{ Slot.Flower, new(new HashSet<Stat>() { Stat.Hp }, SubStats) },
		{ Slot.Plume, new(new HashSet<Stat>() { Stat.Atk }, SubStats) },
		{ Slot.Sands, new(new HashSet<Stat>() { Stat.HpPerc, Stat.AtkPerc, Stat.DefPerc, Stat.ElementalMastery, Stat.EnergyRecharge }, SubStats) },
		{ Slot.Goblet, new(new HashSet<Stat>() { Stat.HpPerc, Stat.AtkPerc, Stat.DefPerc, Stat.ElementalMastery, Stat.PyroDmgBonus, Stat.HydroDmgBonus, Stat.ElectroDmgBonus, Stat.AnemoDmgBonus, Stat.CryoDmgBonus, Stat.GeoDmgBonus, Stat.PhysicalDmgBonus, }, SubStats) },
		{ Slot.Circlet, new(new HashSet<Stat>() { Stat.HpPerc, Stat.AtkPerc, Stat.DefPerc, Stat.ElementalMastery, Stat.CritRate, Stat.CritDmg, Stat.HealingBonusPerc }, SubStats) },
	};
}
