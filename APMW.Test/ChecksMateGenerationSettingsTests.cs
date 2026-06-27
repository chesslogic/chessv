using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Archipelago.APChessV.Generation.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChessV.Test
{
    [TestClass]
    public class ChecksMateGenerationSettingsTests
    {
        [TestMethod]
        public void Defaults_MatchSampleYamlAndReadmeOptions()
        {
            var settings = new ChecksMateGenerationSettings();

            Assert.AreEqual(0, settings.Validate().Count);

            var options = settings.ToYamlOptionMap();
            Assert.AreEqual("0", options[ChecksMateGenerationOptionKeys.ProgressionBalancing]);
            Assert.AreEqual("full", options[ChecksMateGenerationOptionKeys.Accessibility]);
            Assert.AreEqual("ordered_progressive", options[ChecksMateGenerationOptionKeys.Goal]);
            Assert.AreEqual("daily", options[ChecksMateGenerationOptionKeys.Difficulty]);
            Assert.AreEqual("all", options[ChecksMateGenerationOptionKeys.EnableTactics]);
            Assert.AreEqual("stable", options[ChecksMateGenerationOptionKeys.PieceLocations]);
            Assert.AreEqual("stable", options[ChecksMateGenerationOptionKeys.PieceTypes]);
            Assert.AreEqual("off", options[ChecksMateGenerationOptionKeys.EarlyMaterial]);
            Assert.AreEqual("random-high", options[ChecksMateGenerationOptionKeys.MaxEnginePenalties]);
            Assert.AreEqual("random-high", options[ChecksMateGenerationOptionKeys.MaxPocket]);
            Assert.AreEqual("random", options[ChecksMateGenerationOptionKeys.MaxKings]);
            Assert.AreEqual("2", options[ChecksMateGenerationOptionKeys.FairyKings]);
            Assert.AreEqual("full", options[ChecksMateGenerationOptionKeys.FairyChessPieces]);
            Assert.AreEqual("chaos", options[ChecksMateGenerationOptionKeys.FairyChessArmy]);
            Assert.AreEqual("mixed", options[ChecksMateGenerationOptionKeys.FairyChessPawns]);
            Assert.AreEqual("off", options[ChecksMateGenerationOptionKeys.FairyChessPawnUpgrades]);
            Assert.AreEqual("0", options[ChecksMateGenerationOptionKeys.MinorPieceLimitByType]);
            Assert.AreEqual("0", options[ChecksMateGenerationOptionKeys.MajorPieceLimitByType]);
            Assert.AreEqual("0", options[ChecksMateGenerationOptionKeys.QueenPieceLimitByType]);
            Assert.AreEqual("0", options[ChecksMateGenerationOptionKeys.QueenPieceLimit]);
            Assert.AreEqual("4", options[ChecksMateGenerationOptionKeys.PocketLimitByPocket]);
            Assert.AreEqual(false, options[ChecksMateGenerationOptionKeys.DeathLink]);

            CollectionAssert.AreEqual(
                new[] { "Petal", "Cannon", "Nutty", "Rookies", "FIDE", "Clobberers", "Camel" },
                ((List<string>)options[ChecksMateGenerationOptionKeys.FairyChessPiecesConfigure]).ToArray());

            var asymmetricTrades = (Dictionary<string, int>)options[ChecksMateGenerationOptionKeys.AsymmetricTrades];
            Assert.AreEqual(50, asymmetricTrades["disabled"]);
            Assert.AreEqual(0, asymmetricTrades["jacks"]);
        }

        [TestMethod]
        public void ToYamlCompatibleMap_ExposesTopLevelYamlShape()
        {
            var settings = new ChecksMateGenerationSettings();

            var yaml = settings.ToYamlCompatibleMap();

            Assert.AreEqual(ChecksMateGenerationSettings.DefaultName, yaml["name"]);
            Assert.AreEqual(ChecksMateGenerationSettings.DefaultGame, yaml["game"]);
            Assert.AreEqual(ChecksMateGenerationSettings.DefaultDescription, yaml["description"]);
            Assert.IsTrue(yaml.ContainsKey(ChecksMateGenerationSettings.DefaultGame));

            var requires = (Dictionary<string, object>)yaml["requires"];
            Assert.AreEqual(
                ChecksMateGenerationSettings.DefaultRequiredArchipelagoVersion,
                requires["version"]);
        }

        [TestMethod]
        public void Serialize_Defaults_EmitsExpectedYamlShape()
        {
            var yaml = ChecksMateGenerationSettingsYamlSerializer.Serialize(
                new ChecksMateGenerationSettings());

            StringAssert.Contains(yaml, "name: " + ChecksMateGenerationSettings.DefaultName);
            StringAssert.Contains(yaml, "game: " + ChecksMateGenerationSettings.DefaultGame);
            StringAssert.Contains(yaml, "description: " + ChecksMateGenerationSettings.DefaultDescription);
            StringAssert.Contains(yaml, "requires:");
            StringAssert.Contains(yaml, "  version: " + ChecksMateGenerationSettings.DefaultRequiredArchipelagoVersion);
            StringAssert.Contains(yaml, "ChecksMate:");
            StringAssert.Contains(yaml, "  progression_balancing: '0'");
            StringAssert.Contains(yaml, "  fairy_chess_pieces_configure:");
            StringAssert.Contains(yaml, "    - Petal");
            StringAssert.Contains(yaml, "  death_link: false");
        }

        [TestMethod]
        public void SerializeAndDeserialize_RoundTripsRepresentativeOptions()
        {
            var settings = new ChecksMateGenerationSettings
            {
                Name = "RepresentativeChecksMate",
                Description = "Round trip coverage",
                ProgressionBalancing = 75,
                Accessibility = ChecksMateAccessibility.Items,
                Goal = ChecksMateGoal.Super,
                PieceLocations = ChecksMatePieceLocations.Chaos,
                PieceTypes = ChecksMatePieceTypes.Book,
                MaxEnginePenalties = ChecksMateLimitValue.Fixed(3),
                MaxPocket = ChecksMateLimitValue.RandomLow,
                FairyKings = ChecksMateLimitValue.Fixed(4),
                FairyChessPieces = ChecksMateFairyChessPieces.Configure,
                FairyChessArmy = ChecksMateFairyChessArmy.Limited,
                FairyChessPawns = ChecksMateFairyChessPawns.AnyFairy,
                FairyChessPawnUpgrades = ChecksMateFairyChessPawnUpgrades.Max,
                MinorPieceLimitByType = 1,
                MajorPieceLimitByType = 2,
                QueenPieceLimitByType = 3,
                QueenPieceLimit = 4,
                PocketLimitByPocket = 5,
                DeathLink = true,
            };
            settings.FairyChessPiecesConfigure.Clear();
            settings.FairyChessPiecesConfigure.Add(ChecksMateFairyChessPieceSet.Fide);
            settings.FairyChessPiecesConfigure.Add(ChecksMateFairyChessPieceSet.Camel);
            settings.AsymmetricTrades.Clear();
            settings.AsymmetricTrades[ChecksMateAsymmetricTrade.Disabled] = 10;
            settings.AsymmetricTrades[ChecksMateAsymmetricTrade.Jacks] = 20;

            var yaml = ChecksMateGenerationSettingsYamlSerializer.Serialize(settings);
            var loaded = ChecksMateGenerationSettingsYamlSerializer.Deserialize(yaml);

            Assert.AreEqual(settings.Name, loaded.Name);
            Assert.AreEqual(settings.Description, loaded.Description);
            Assert.AreEqual(settings.ProgressionBalancing, loaded.ProgressionBalancing);
            Assert.AreEqual(settings.Accessibility, loaded.Accessibility);
            Assert.AreEqual(settings.Goal, loaded.Goal);
            Assert.AreEqual(settings.PieceLocations, loaded.PieceLocations);
            Assert.AreEqual(settings.PieceTypes, loaded.PieceTypes);
            Assert.AreEqual(settings.MaxEnginePenalties.ToYamlValue(), loaded.MaxEnginePenalties.ToYamlValue());
            Assert.AreEqual(settings.MaxPocket.ToYamlValue(), loaded.MaxPocket.ToYamlValue());
            Assert.AreEqual(settings.FairyKings.ToYamlValue(), loaded.FairyKings.ToYamlValue());
            Assert.AreEqual(settings.FairyChessPieces, loaded.FairyChessPieces);
            CollectionAssert.AreEqual(
                settings.FairyChessPiecesConfigure.ToArray(),
                loaded.FairyChessPiecesConfigure.ToArray());
            Assert.AreEqual(settings.FairyChessArmy, loaded.FairyChessArmy);
            Assert.AreEqual(settings.FairyChessPawns, loaded.FairyChessPawns);
            Assert.AreEqual(settings.FairyChessPawnUpgrades, loaded.FairyChessPawnUpgrades);
            Assert.AreEqual(settings.MinorPieceLimitByType, loaded.MinorPieceLimitByType);
            Assert.AreEqual(settings.MajorPieceLimitByType, loaded.MajorPieceLimitByType);
            Assert.AreEqual(settings.QueenPieceLimitByType, loaded.QueenPieceLimitByType);
            Assert.AreEqual(settings.QueenPieceLimit, loaded.QueenPieceLimit);
            Assert.AreEqual(settings.PocketLimitByPocket, loaded.PocketLimitByPocket);
            Assert.AreEqual(settings.DeathLink, loaded.DeathLink);
            Assert.AreEqual(10, loaded.AsymmetricTrades[ChecksMateAsymmetricTrade.Disabled]);
            Assert.AreEqual(20, loaded.AsymmetricTrades[ChecksMateAsymmetricTrade.Jacks]);
        }

        [TestMethod]
        public void SaveAndLoad_RoundTripsYamlFile()
        {
            var directoryPath = Path.Combine(
                Environment.CurrentDirectory,
                "ChecksMateGenerationSettingsTestData",
                Guid.NewGuid().ToString("N"));
            var filePath = Path.Combine(directoryPath, "Player.yaml");

            try
            {
                Directory.CreateDirectory(directoryPath);
                var settings = new ChecksMateGenerationSettings
                {
                    Name = "SavedChecksMate",
                    Description = "Saved YAML file round trip",
                    ProgressionBalancing = 33,
                    Goal = ChecksMateGoal.Progressive,
                    DeathLink = true,
                };

                ChecksMateGenerationSettingsYamlSerializer.Save(filePath, settings);
                var loaded = ChecksMateGenerationSettingsYamlSerializer.Load(filePath);

                Assert.IsTrue(File.Exists(filePath), filePath);
                Assert.AreEqual(settings.Name, loaded.Name);
                Assert.AreEqual(settings.Description, loaded.Description);
                Assert.AreEqual(settings.ProgressionBalancing, loaded.ProgressionBalancing);
                Assert.AreEqual(settings.Goal, loaded.Goal);
                Assert.AreEqual(settings.DeathLink, loaded.DeathLink);
            }
            finally
            {
                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                }
            }
        }

        [TestMethod]
        public void Deserialize_ReportsValidationFailures()
        {
            var yaml = @"name: BadChecksMate
game: ChecksMate
description: Invalid progression balancing
requires:
  version: 0.5.1
ChecksMate:
  progression_balancing: '101'";

            var exception = Assert.ThrowsException<ChecksMateGenerationSettingsYamlException>(
                () => ChecksMateGenerationSettingsYamlSerializer.Deserialize(yaml));

            StringAssert.Contains(exception.Message, nameof(ChecksMateGenerationSettings.ProgressionBalancing));
            StringAssert.Contains(exception.Message, "between 0 and 100");
        }

        [TestMethod]
        public void Deserialize_SupportsSampleStyleInlineListsAndComments()
        {
            var yaml = @"Generated item summary can precede the YAML.
-----
name: HakknivChess{number}
game: ChecksMate
description: All pieces, stable board
requires:
  version: 0.5.1 # Version comment
ChecksMate:
  enable_tactics: all # option comment
  fairy_chess_pieces: configure
  fairy_chess_pieces_configure:
    ['Petal', 'Cannon', 'FIDE']
  asymmetric_trades:
    disabled: 50
    jacks: 0";

            var settings = ChecksMateGenerationSettingsYamlSerializer.Deserialize(yaml);

            Assert.AreEqual(ChecksMateFairyChessPieces.Configure, settings.FairyChessPieces);
            CollectionAssert.AreEqual(
                new[]
                {
                    ChecksMateFairyChessPieceSet.Petal,
                    ChecksMateFairyChessPieceSet.Cannon,
                    ChecksMateFairyChessPieceSet.Fide,
                },
                settings.FairyChessPiecesConfigure.ToArray());
            Assert.AreEqual(50, settings.AsymmetricTrades[ChecksMateAsymmetricTrade.Disabled]);
            Assert.AreEqual(0, settings.AsymmetricTrades[ChecksMateAsymmetricTrade.Jacks]);
        }

        [TestMethod]
        public void Deserialize_PreservesUnsupportedFieldsWhenSaving()
        {
            var yaml = @"name: CustomChecksMate
game: ChecksMate
description: Preserve unknown options
requires:
  version: 0.5.1
  generator: custom
metadata: keep-top-level
ChecksMate:
  progression_balancing: 25
  custom_option: keep-me
  custom_map:
    child: 7";

            var settings = ChecksMateGenerationSettingsYamlSerializer.Deserialize(yaml);

            Assert.AreEqual("custom", settings.UnsupportedRequiresFields["generator"]);
            Assert.AreEqual("keep-top-level", settings.UnsupportedTopLevelFields["metadata"]);
            Assert.AreEqual("keep-me", settings.UnsupportedChecksMateOptions["custom_option"]);
            Assert.IsTrue(settings.UnsupportedChecksMateOptions.ContainsKey("custom_map"));

            var serialized = ChecksMateGenerationSettingsYamlSerializer.Serialize(settings);
            StringAssert.Contains(serialized, "  generator: custom");
            StringAssert.Contains(serialized, "metadata: keep-top-level");
            StringAssert.Contains(serialized, "  custom_option: keep-me");
            StringAssert.Contains(serialized, "  custom_map:");
            StringAssert.Contains(serialized, "    child: 7");

            var roundTripped = ChecksMateGenerationSettingsYamlSerializer.Deserialize(serialized);
            Assert.AreEqual("keep-me", roundTripped.UnsupportedChecksMateOptions["custom_option"]);
            Assert.IsTrue(roundTripped.UnsupportedChecksMateOptions.ContainsKey("custom_map"));
        }

        [TestMethod]
        public void Validate_ReportsRequiredFieldsInvalidEnumsRangesAndWeights()
        {
            var settings = new ChecksMateGenerationSettings
            {
                Name = " ",
                Game = "ChessV",
                RequiredArchipelagoVersion = "not-a-version",
                ProgressionBalancing = 101,
                Goal = (ChecksMateGoal)99,
                FairyChessPieces = ChecksMateFairyChessPieces.Configure,
                MinorPieceLimitByType = -1,
            };
            settings.FairyChessPiecesConfigure.Clear();
            settings.AsymmetricTrades[ChecksMateAsymmetricTrade.Disabled] = -1;

            var errors = settings.Validate();
            var propertyNames = errors.Select(error => error.PropertyName).ToList();

            Assert.IsTrue(propertyNames.Contains(nameof(ChecksMateGenerationSettings.Name)));
            Assert.IsTrue(propertyNames.Contains(nameof(ChecksMateGenerationSettings.Game)));
            Assert.IsTrue(propertyNames.Contains(nameof(ChecksMateGenerationSettings.RequiredArchipelagoVersion)));
            Assert.IsTrue(propertyNames.Contains(nameof(ChecksMateGenerationSettings.ProgressionBalancing)));
            Assert.IsTrue(propertyNames.Contains(nameof(ChecksMateGenerationSettings.Goal)));
            Assert.IsTrue(propertyNames.Contains(nameof(ChecksMateGenerationSettings.FairyChessPiecesConfigure)));
            Assert.IsTrue(propertyNames.Contains(nameof(ChecksMateGenerationSettings.MinorPieceLimitByType)));
            Assert.IsTrue(propertyNames.Contains(nameof(ChecksMateGenerationSettings.AsymmetricTrades)));
            Assert.ThrowsException<InvalidOperationException>(() => settings.ToYamlOptionMap());
        }
    }
}
