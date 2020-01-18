using System;
using System.Data;
using System.Linq;
using System.Reflection;

using HibernatingRhinos.Profiler.Appender.NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Linq;

namespace NHibernateTutorial2
{
    internal class Program
    {
        private static void Main()
        {
            var cfg = ConfigureNHibernate();
            var sessionFactory = cfg.BuildSessionFactory();

            Guid id;
            using (var session = sessionFactory.OpenSession())

            // transaction to create customers and orders
            using (var tx = session.BeginTransaction())
            {
                var newCustomer = CreateCustomer();
                Console.WriteLine("New Customer:");
                Console.WriteLine(newCustomer);
                session.Save(newCustomer);

                /* Manually having to save Orders for the customer here 
                foreach (var order in newCustomer.Orders)
                {
                    session.Save(order);
                }
                */

                id = newCustomer.Id;
                tx.Commit();
            }

            using (var session = sessionFactory.OpenSession())

            // transaction to bring up (load) existing customers and orders
            using (var tx = session.BeginTransaction())
            {
                /* join and fetch happening at once at query level */
                var query = from customer in session.Query<Customer>()
                            where customer.Id == id
                            select customer;
                var reloaded = query.Fetch(x => x.Orders).ToList().First();

                //var reloaded = session.Load<Customer>(id);
                Console.WriteLine("Reloaded:");
                Console.WriteLine(reloaded);
                
                /*
                Console.WriteLine("The orders were ordered by: ");

                foreach (var order in reloaded.Orders)
                {
                    Console.WriteLine(order.Customer);
                }
                */

                tx.Commit();
            }

            Console.WriteLine("Press <ENTER> to exit...");
            Console.ReadLine();
        }

        private static Customer CreateCustomer()
        {
            var customer = new Customer
            {
                FirstName = "John",
                LastName = "Doe",
                Points = 100,
                HasGoldStatus = true,
                MemberSince = new DateTime(2012, 1, 1),
                CreditRating = CustomerCreditRating.Good,
                AverageRating = 42.42424242,
                Address = CreateLocation()
            };

            var order1 = new Order
            {
                Ordered = DateTime.Now
            };

            customer.AddOrder(order1);

            var order2 = new Order
            {
                Ordered = DateTime.Now.AddDays(-1),
                Shipped = DateTime.Now,
                ShipTo = CreateLocation()
            };

            customer.AddOrder(order2);
            return customer;
        }

        private static Location CreateLocation()
        {
            return new Location
            {
                Street = "123 Somewhere Avenue",
                City = "Nowhere",
                Province = "Alberta",
                Country = "Canada"
            };
        }

        private static Configuration ConfigureNHibernate()
        {
            NHibernateProfiler.Initialize();
            var cfg = new Configuration();

            cfg.DataBaseIntegration(x =>
            {
                x.ConnectionStringName = "default";
                x.Driver<SqlClientDriver>();
                x.Dialect<MsSql2008Dialect>();
                x.IsolationLevel = IsolationLevel.RepeatableRead;
                x.Timeout = 10; 
                x.BatchSize = 10;
            });

            cfg.SessionFactory().GenerateStatistics();
            cfg.AddAssembly(Assembly.GetExecutingAssembly());
            return cfg;
        }
    }
}
