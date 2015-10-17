using NinjaDomain.Classes;
using NinjaDomain.DataModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            // Stop EF going through the db initialization process when working with the NinjaContext
            Database.SetInitializer(new NullDatabaseInitializer<NinjaContext>());

            // reset db
            //DataHelpers.NewDbWithSeed();
        }

        private static void InsertNinja()
        {
            var ninja = new Ninja
            {
                Name = "PenelopeSan",
                ServedInOniwaban = false,
                DateOfBirth = new DateTime(2015, 01, 08),
                ClanId = 1
            };

            using (var context = new NinjaContext())
            {
                // point the database log function to Console, so we can see the output
                context.Database.Log = Console.WriteLine;

                // Calls sql INSERT function - INSERT [dbo].[Ninjas]([Name], [ServedInOniwaban], [ClanId], [DateOfBirth])
                context.Ninjas.Add(ninja);

                // This must be called, or changes to the context will not be applied to the actual db
                context.SaveChanges();
            }
        }

        private static void InsertMultipleNinjas()
        {
            var ninja1 = new Ninja
            {
                Name = "Leonardo",
                ServedInOniwaban = false,
                DateOfBirth = new DateTime(1984, 10, 02),
                ClanId = 1
            };

            var ninja2 = new Ninja
            {
                Name = "Raphael",
                ServedInOniwaban = false,
                DateOfBirth = new DateTime(1984, 10, 02),
                ClanId = 1
            };

            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                // Same INSERT as in InsertNinja, but doen multiple times in one db call
                context.Ninjas.AddRange(new List<Ninja> { ninja1, ninja2 });
                context.SaveChanges();
            }
        }

        private static void SimpleNinjaQueries()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                // 'Where' returns IEnumerable
                //var ninjas = context.Ninjas.Where(n => n.Name == "Raphael");
                
                // FirstOrDefault returns single object or null
                //var ninja = context.Ninjas.Where(n => n.DateOfBirth >= new DateTime(1984, 1, 1)).FirstOrDefault();
                
                // can combine executing methods with predicates, here using firstordefault with a filter lambda
                //var ninja = context.Ninjas.FirstOrDefault(n => n.DateOfBirth >= new DateTime(1984, 1, 1));
                
                // A paging example
                // Uses SQL SELECT [Extent1.*]... WHERE [Extent1].[DateOfBirth] >= convert(datetime2, '1984-01-01 00:00:00.0000000', 121) ORDER BY [Extent1].[Name] ASC OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY
                var ninja = context.Ninjas.Where(n => n.DateOfBirth >= new DateTime(1984, 1, 1))
                    .OrderBy(n => n.Name)
                    .Skip(1).Take(1)        // Paging methods
                    .FirstOrDefault();      // Executes the query
                
                //var query = context.Ninjas;
                //var someNinjas = query.ToList();

                Console.WriteLine(ninja.Name);

                // the db query will be open for the duration of this foreach loop
                // avoid doing lots of work in a for loop that executes queries
                //foreach (var ninja in ninjas)
                //{
                //    Console.WriteLine(ninja.Name);
                //}
            }
        }

        private static void QueryAndUpdateNinja()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                // first call to db: SELECT TOP 1 FROM Ninjas
                var ninja = context.Ninjas.FirstOrDefault();

                // context modified, not db
                ninja.ServedInOniwaban = !ninja.ServedInOniwaban;

                // second call to db: UPDATE Ninjas SET ServedInOniwaban = 0 WHERE Id = 1
                context.SaveChanges();
            }
        }

        private static void QueryAndUpdateNinjaDisconnected()
        {
            Ninja ninja;

            // represents a service or WebAPI retrieving the data and sending it to the client
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                ninja = context.Ninjas.FirstOrDefault();
            }

            // client modifies data
            ninja.ServedInOniwaban = (!ninja.ServedInOniwaban);

            // reinstantiate the context and call save changes
            // completely new context, not aware of ninja, so won't save changes
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                // add the ninja to the context to fix above problem
                
                // this doesn't look for a matching ID and update, it will INSERT a new row
                //context.Ninjas.Add(ninja);
                
                // tells EF to watch this data
                context.Ninjas.Attach(ninja);

                // tell the context that the entity has changed, and the object should be updated in the DB
                context.Entry(ninja).State = EntityState.Modified;

                context.SaveChanges();
            }

        }

        private static void RetrieveDataWithFind()
        {
            var keyVal = 10;

            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                
                // takes a single value and uses it in the query against the key property
                // will first check to see if the object already exists in memory and is being tracked by the context
                // doesn't waste a trip to the db if the object is found
                var ninja = context.Ninjas.Find(keyVal);
                Console.WriteLine("After Find#1: " + ninja.Name);

                // Ninja found in context, no need for second trip to db
                var someNinja = context.Ninjas.Find(keyVal);
                Console.WriteLine("After Find#2: " + someNinja.Name);
                ninja = null;
            }
        }

        // TODO - couldn't save a stored procedure for some reason
        private static void RetrieveDataWithStoredProc()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                // executes a stored procedure
                // won't be executed until used in the for loop, unless .ToList() is added, because it's just a query
                var ninjas = context.Ninjas.SqlQuery("exec GetOldNinjas");

                foreach(var ninja in ninjas)
                {
                    Console.WriteLine(ninja.Name);
                }
            }
        }

        private static void DeleteNinja()
        {
            Ninja ninja;

            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                // SELECT TOP 1 FROM Ninjas
                ninja = context.Ninjas.FirstOrDefault();
            }

            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                //context.Ninjas.Attach(ninja);
                //context.Ninjas.Remove(ninja);   // new context, without Attach ninja not found
                context.Entry(ninja).State = EntityState.Deleted;

                // DELETE Ninjas WHERE Id = 8
                context.SaveChanges();
            }

            //using (var context = new NinjaContext())
            //{
            //    context.Database.Log = Console.WriteLine;

            //    var ninja = context.Ninjas.FirstOrDefault();
            //    context.Ninjas.Remove(ninja);
            //    context.SaveChanges();
            //}
        }

        // delete a db entry using 2 calls, 1 to find, 1 to apply delete
        private static void DeleteNinjaWithKeyValue()
        {
            var keyVal = 1;

            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                var ninja = context.Ninjas.Find(keyVal);    // db round trip #1
                context.Ninjas.Remove(ninja);
                context.SaveChanges();                      // db round trip #2
            }
        }

        // use a stored procedure to delete an entry in a single db call
        private static void DeleteNinjaViaStoredProcedure()
        {
            var keyVal = 3;

            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                context.Database.ExecuteSqlCommand("exec DeleteNinjaViaId {0}", keyVal);
            }
        }

        private static void InsertNinjaWithEquipment()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                var ninja = new Ninja
                {
                    Name = "Kacy Catanzaro",
                    ServedInOniwaban = false,
                    DateOfBirth = new DateTime(1990, 1, 14),
                    ClanId = 1
                };

                var muscles = new NinjaEquipment
                {
                    Name = "Muscles",
                    Type = EquipmentType.Tool,

                };

                // lol
                var spunk = new NinjaEquipment
                {
                    Name = "Spunk",
                    Type = EquipmentType.Weapon
                };

                context.Ninjas.Add(ninja);
                ninja.EquipmentOwned.Add(muscles);
                ninja.EquipmentOwned.Add(spunk);
                context.SaveChanges();
            }
        }

        private static void SimpleNinjaGraphQuery()
        {
            using(var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                // Eager Loading
                // left outer join, returns first Ninja that matches the predicate, Includes associated equipment
                //var ninja = context.Ninjas.Include(n => n.EquipmentOwned).FirstOrDefault(n => n.Name.StartsWith("Kacy"));

                // Explicit Loading
                // first db trip, just get the Ninja
                var ninja = context.Ninjas.FirstOrDefault(n => n.Name.StartsWith("Kacy"));
                Console.WriteLine("Ninja retrieved: " + ninja.Name);
                // second db trip, get the related collection
                // load causes the query to be executed right away, in the same way FirstOrDefault does
                //context.Entry(ninja).Collection(n => n.EquipmentOwned).Load();

                // Lazy loading, 
                // related collections automatically loaded from the database the first time that a property referring to the entity/entities is accessed
                // requires that the related collections are virtual:
                // Ninja { public virtual List<NinjaEquipment> EquipmentOwned { get; set; } }
                Console.WriteLine("Ninja Equipment Count: {0}", ninja.EquipmentOwned.Count());
            }
        }

        // queries that return something other than complete entities - returns anonymous type
        private static void ProjectionQuery()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                var ninjas = context.Ninjas.Select(n => new { n.Name, n.DateOfBirth, n.EquipmentOwned }).ToList();
            }
        }

        // used to prepare db for part 4
        private static void ReseedDatabase()
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<NinjaContext>());
            using (var context = new NinjaContext())
            {
                context.Clans.Add(new Clan { ClanName = "Vermont Clan" });
                var j = new Ninja
                {
                    Name = "JulieSan",
                    ServedInOniwaban = false,
                    DateOfBirth = new DateTime(1980, 1, 1),
                    ClanId = 1

                };
                var s = new Ninja
                {
                    Name = "SampsonSan",
                    ServedInOniwaban = false,
                    DateOfBirth = new DateTime(2008, 1, 28),
                    ClanId = 1

                };
                var l = new Ninja
                {
                    Name = "Leonardo",
                    ServedInOniwaban = false,
                    DateOfBirth = new DateTime(1984, 1, 1),
                    ClanId = 1
                };
                var r = new Ninja
                {
                    Name = "Raphael",
                    ServedInOniwaban = false,
                    DateOfBirth = new DateTime(1985, 1, 1),
                    ClanId = 1
                };
                context.Ninjas.AddRange(new List<Ninja> { j, s, l, r });
                context.SaveChanges();
                context.Database.ExecuteSqlCommand(
                  @"CREATE PROCEDURE GetOldNinjas
                    AS  SELECT * FROM Ninjas WHERE DateOfBirth<='1/1/1980'");

                context.Database.ExecuteSqlCommand(
                   @"CREATE PROCEDURE DeleteNinjaViaId
                     @Id int
                     AS
                     DELETE from Ninjas Where Id = @id
                     RETURN @@rowcount");
            }

        }

    }
}
