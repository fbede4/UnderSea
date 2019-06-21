﻿using Microsoft.EntityFrameworkCore;
using StrategyGame.Bll;
using StrategyGame.Bll.EffectParsing;
using StrategyGame.Bll.Extensions;
using StrategyGame.Dal;
using StrategyGame.Model.Entities;
using StrategyGame.Model.Entities.Frontend;
using System;
using System.Threading;

namespace StrategyGame.DatabaseHelper
{
    class Program
    {
        static void Main()
        {
            var connString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=UnderSeaDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            Console.WriteLine("Connecting to database with connection string: " + connString);

            var builder = new DbContextOptionsBuilder<UnderSeaDatabaseContext>();
            // local DB
            builder.UseSqlServer(connString);

            using (var context = new UnderSeaDatabaseContext(builder.Options))
            {
                Console.WriteLine("Connected to the database.");

                PurgeDatabase(context);
                FillWithDefault(context);                               
            }

            Console.Write("Finished, press any key to exit...");
            Console.ReadKey(true);
        }

        static void PurgeDatabase(UnderSeaDatabaseContext context)
        {
            Console.WriteLine("Purging the database...");

            // Purge connecting tables
            context.BuildingEffects.RemoveRange(context.BuildingEffects);
            context.ResearchEffects.RemoveRange(context.ResearchEffects);
            context.InProgressBuildings.RemoveRange(context.InProgressBuildings);
            context.InProgressResearches.RemoveRange(context.InProgressResearches);
            context.CountryBuildings.RemoveRange(context.CountryBuildings);
            context.CountryResearches.RemoveRange(context.CountryResearches);

            // Purge contents
            context.BuildingContents.RemoveRange(context.BuildingContents);
            context.ResearchContents.RemoveRange(context.ResearchContents);
            context.UnitContents.RemoveRange(context.UnitContents);

            // Purge commands, divisions, and units
            context.Commands.RemoveRange(context.Commands);
            context.Divisions.RemoveRange(context.Divisions);
            context.UnitTypes.RemoveRange(context.UnitTypes);

            // Purge "static" values
            context.BuildingTypes.RemoveRange(context.BuildingTypes);
            context.Countries.RemoveRange(context.Countries);
            context.Effects.RemoveRange(context.Effects);
            context.GlobalValues.RemoveRange(context.GlobalValues);
            context.ResearchTypes.RemoveRange(context.ResearchTypes);

            // Purge users
            context.Users.RemoveRange(context.Users);
            context.SaveChanges();

            Console.WriteLine("Database purged!");
        }

        static void FillWithDefault(UnderSeaDatabaseContext context)
        {
            Console.WriteLine("Adding default data...");

            // Effects, Buildings, researches
            // áramlásirányító
            var popIn = new Effect() { Name = KnownValues.PopulationIncrease, Value = 50 };
            var cp = new Effect() { Name = KnownValues.CoralProductionIncrease, Value = 200 };
            var currentController = new BuildingType() { CostPearl = 1000, CostCoral = 0, BuildTime = 5, MaxCount = -1 };

            // zátonyvár
            var bsIn = new Effect() { Name = KnownValues.BarrackSpaceIncrease, Value = 200 };
            var reefCastle = new BuildingType() { CostPearl = 1000, CostCoral = 0, BuildTime = 5, MaxCount = -1 };

            // Iszaptraktor
            var harvMod1 = new Effect() { Name = KnownValues.HarvestModifier, Value = 0.1 };
            var mudT = new ResearchType() { CostPearl = 0, CostCoral = 0, ResearchTime = 15, MaxCompletedAmount = 1 };

            // Iszapkombájn
            var harvMod2 = new Effect() { Name = KnownValues.HarvestModifier, Value = 0.15 };
            var mudC = new ResearchType() { CostPearl = 0, CostCoral = 0, ResearchTime = 15, MaxCompletedAmount = 1 };

            // korallfal
            var defMod1 = new Effect() { Name = KnownValues.UnitDefenseModifier, Value = 0.2 };
            var wall = new ResearchType() { CostPearl = 0, CostCoral = 0, ResearchTime = 15, MaxCompletedAmount = 1 };

            // Szonárágyú
            var attMod1 = new Effect() { Name = KnownValues.UnitAttackModifier, Value = 0.2 };
            var canon = new ResearchType() { CostPearl = 0, CostCoral = 0, ResearchTime = 15, MaxCompletedAmount = 1 };

            // Harcművészet
            var combModA = new Effect() { Name = KnownValues.UnitAttackModifier, Value = 0.1 };
            var combModD = new Effect() { Name = KnownValues.UnitDefenseModifier, Value = 0.1 };
            var martialArts = new ResearchType() { CostPearl = 0, CostCoral = 0, ResearchTime = 15, MaxCompletedAmount = 1 };

            // Alchemy
            var taxMod1 = new Effect() { Name = KnownValues.TaxationModifier, Value = 0.3 };
            var alchemy = new ResearchType() { CostPearl = 0, CostCoral = 0, ResearchTime = 15, MaxCompletedAmount = 1 };


            // Add effects, buildings, researches
            context.Effects.AddRange(popIn, cp, bsIn, harvMod1, harvMod2,
                defMod1, attMod1, combModA, combModD, taxMod1);
            context.BuildingTypes.AddRange(currentController, reefCastle);
            context.ResearchTypes.AddRange(mudT, mudC, wall, canon, martialArts, alchemy);
            context.SaveChanges();


            // Add effects to buildings and researches
            context.BuildingEffects.AddRange(
                new BuildingEffect() { Building = currentController, Effect = popIn },
                new BuildingEffect() { Building = currentController, Effect = cp },
                new BuildingEffect() { Building = reefCastle, Effect = bsIn });

            context.ResearchEffects.AddRange(
                new ResearchEffect() { Research = mudT, Effect = harvMod1 },
                new ResearchEffect() { Research = mudC, Effect = harvMod2 },
                new ResearchEffect() { Research = wall, Effect = defMod1 },
                new ResearchEffect() { Research = canon, Effect = attMod1 },
                new ResearchEffect() { Research = martialArts, Effect = combModA },
                new ResearchEffect() { Research = martialArts, Effect = combModD },
                new ResearchEffect() { Research = alchemy, Effect = taxMod1 });
            context.SaveChanges();


            // Add units
            // rohamfóka
            var seal = new UnitType()
            { AttackPower = 6, DefensePower = 2, CostPearl = 50, CostCoral = 0, MaintenancePearl = 1, MaintenanceCoral = 1 };
            // csatacsikó
            var pony = new UnitType()
            { AttackPower = 2, DefensePower = 6, CostPearl = 50, CostCoral = 0, MaintenancePearl = 1, MaintenanceCoral = 1 };
            // lézercápa
            var lazor = new UnitType()
            { AttackPower = 5, DefensePower = 5, CostPearl = 100, CostCoral = 0, MaintenancePearl = 3, MaintenanceCoral = 2 };

            context.UnitTypes.AddRange(seal, pony, lazor);
            context.SaveChanges();


            // Add contents
            var currentCont = new BuildingContent()
            {
                Parent = currentController,
                Name = "Áramlásirányító",
                Description = "+50 lakos, 200 korall / kör",
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTpStWdiTJFGARYo6i93yeO0pHA0EQYJelOifiWIPmP7qveLS6n"
            };

            var reefCastCont = new BuildingContent()
            {
                Parent = reefCastle,
                Name = "Zátonyvár",
                Description = "+200 szállás",
                ImageUrl = "https://media-cdn.tripadvisor.com/media/photo-s/02/72/f4/54/filename-pict0458-jpg.jpg"
            };

            context.BuildingContents.AddRange(currentCont, reefCastCont);

            var sealCont = new UnitContent()
            {
                Parent = seal,
                Name = "Rohamfóka",
                Description = "Jól támad de rosszul véd",
                ImageUrl = "https://resources.stuff.co.nz/content/dam/images/1/t/a/s/4/o/image.related.StuffLandscapeSixteenByNine.710x400.1tankf.png/1546211918775.jpg"
            };
            var ponyCont = new UnitContent()
            {
                Parent = pony,
                Name = "Csatacsikó",
                Description = "Jól véd de rosszul támad",
                ImageUrl = "http://www2.padi.com/blog/wp-content/uploads/2013/08/seahorse.jpg"
            };
            var lazorCont = new UnitContent()
            {
                Parent = lazor,
                Name = "Lézercápa",
                Description = "lazers man",
                ImageUrl = "https://vignette.wikia.nocookie.net/venturian-battle-headquarters/images/6/69/Flyinglasershark.jpg/revision/latest?cb=20160714220743"
            };

            context.UnitContents.AddRange(sealCont, ponyCont, lazorCont);

            var mudTCont = new ResearchContent()
            {
                Parent = mudT,
                Name = "Iszap traktor",
                Description = "Iszapozza a korallt (amitől amúgy IRL meghalna, korall nem növény nem kell neki föld), +10% korall termelés",
                ImageUrl = "https://cdn.pixabay.com/photo/2017/10/09/09/55/mud-2832910_960_720.jpg"
            };
            var mudCCont = new ResearchContent()
            {
                Parent = mudC,
                Name = "Iszap kombájn",
                Description = "Nagyon iszapozza a korallt, +15% korall termelés",
                ImageUrl = "https://secure.i.telegraph.co.uk/multimedia/archive/03350/glastonbury-mud-sp_3350460k.jpg"
            };
            var defCont = new ResearchContent()
            {
                Parent = wall,
                Name = "Korallfal",
                Description = "Fal, korallból. +20% védekezés",
                ImageUrl = "https://ak2.picdn.net/shutterstock/videos/1396612/thumb/1.jpg"
            };
            var attCont = new ResearchContent()
            {
                Parent = canon,
                Name = "Szonárágyú",
                Description = "Mint a denevér, echo-lokáció. +20% támadás",
                ImageUrl = "https://media-cdn.tripadvisor.com/media/photo-s/07/24/69/da/dive-abaco.jpg"
            };
            var cCont = new ResearchContent()
            {
                Parent = martialArts,
                Name = "Vízalatti harcművészetek",
                Description = "\"A különbség a lehetetlen és a lehetséges között az egyén akarata.\", +10% védekezés és támadás",
                ImageUrl = "https://www.pallensmartialarts.com/uploads/1/0/9/3/109303993/girl-kicking-boy-to-air_1_orig.jpg"
            };
            var taxCont = new ResearchContent()
            {
                Parent = alchemy,
                Name = "Alkímia",
                Description = "A népesség pénzt csinál, +30% adó bevétel",
                ImageUrl = "https://f4.bcbits.com/img/a3431451072_10.jpg"
            };

            context.ResearchContents.AddRange(mudTCont, mudCCont, defCont, attCont, cCont, taxCont);
            context.SaveChanges();


            // globals
            context.GlobalValues.Add(new GlobalValue()
            {
                BaseTaxation = 25,
                Round = 1,
                StartingBarrackSpace = 100,
                StartingCorals = 500,
                StartingPearls = 500,
                StartingPopulation = 10
            });
            context.SaveChanges();


            // Add some test data
            var user1 = new User() { Email = "poi@poi.hu", UserName = "poi" };
            var user2 = new User() { Email = "hue@poi.hu", UserName = "hue" };
            context.Users.AddRange(user1, user2);

            var country1 = new Country() { Pearls = 5000, Corals = 5000, ParentUser= user1 };
            var country2 = new Country() { Pearls = 5000, Corals = 5000, ParentUser = user2 };
            context.Countries.AddRange(country1, country2);

            var def = new Command() { ParentCountry = country1, TargetCountry = country1 };
            context.Commands.AddRange(new Command() { ParentCountry = country2, TargetCountry = country2 }, def);
            context.SaveChanges();

            context.TryStartBuildingAsync(country1.Id, currentController).Wait();
            context.TryStartBuildingAsync(country2.Id, reefCastle).Wait();
            context.TryStartResearchAsync(country2.Id, martialArts).Wait();

            var handler = new GlobalTurnHandler(new CountryTurnHandler(new ModifierParserContainer(new AbstractEffectModifierParser[]
            {
                new BarrackSpaceEffectParser(),
                new CoralProductionEffectParser(),
                new HarvestModifierEffectParser(),
                new PopulationEffectParser(),
                new TaxModifierEffectParser(),
                new UnitDefenseEffectParser(),
                new UnitAttackEffectParser()
            })));

            for (int i = 0; i < 6; i++)
            {
                handler.EndTurnAsync(context, CancellationToken.None).Wait();
            }
            for (int i = 0; i < 15; i++)
            {
                handler.EndTurnAsync(context, CancellationToken.None).Wait();
            }
            var attack = new Command() { ParentCountry = country2, TargetCountry = country1 };
            var div = new Division() { ParentCommand = attack, Count = 20, Unit = lazor };
            var div2 = new Division() { ParentCommand = def, Count = 20, Unit = lazor };

            context.Commands.Add(attack);
            context.Divisions.AddRange(div, div2);

            handler.EndTurnAsync(context, CancellationToken.None).Wait();

            Console.WriteLine();
        }
    }
}