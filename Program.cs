using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

using HibernatingRhinos.Profiler.Appender.NHibernate;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;
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
            using (var session = sessionFactory.OpenSession())

            /* Native SQL in NHibernate demonstration 
            using (var tx = session.BeginTransaction())
            {
                IQuery sqlQuery = session.CreateSQLQuery("SELECT * FROM customer").AddEntity(typeof(Customer));
                var customers = sqlQuery.List<Customer>();

                //IList<Customer> customers = session.CreateSQLQuery("SELECT * FROM customer")
                //   .AddScalar("Id", NHibernateUtil.Guid)
                //   .AddScalar("FirstName", NHibernateUtil.String)
                //   .AddScalar("LastName", NHibernateUtil.String).List<Customer>();

                //IList<Customer> customers = session.CreateSQLQuery("SELECT * FROM customer WHERE FirstName = 'Laverne'") 
                //   .AddEntity(typeof(Customer)).List<Customer>();

                foreach (var customer in customers)
                {
                    Console.WriteLine(customer);
                }

                tx.Commit();
            }
            */

            /* QueryOver demonstration 
            using (var tx = session.BeginTransaction())
            {
                var customers = session.QueryOver<Customer>().Where(x => x.FirstName == "John");
                //var customers = session.QueryOver<Customer>().Where(Restrictions.On<Customer>(c => c.FirstName).IsLike("A%"));

                foreach (var customer in customers.List())
                {
                    Console.WriteLine(customer);
                }

                tx.Commit();
            }
            */

            /* NHibernate Query by Criteria API demonstration
            using (var tx = session.BeginTransaction())
            {
                var customers = session.CreateCriteria<Customer>().Add(Restrictions.Like("FirstName", "J%"));
                //var customers = session.CreateCriteria<Customer>().Add(Restrictions.Eq("FirstName", "John")).List<Customer>();


                foreach (var customer in customers.List<Customer>())
                {
                    Console.WriteLine(customer);
                }

                tx.Commit();
            }
            */

            /* Hibernate Query Language demonstration (HQL)
            using (var tx = session.BeginTransaction())
            {
                var customers = session.CreateQuery("select c from Customer c where c.FirstName = 'John'");
                // var customers = session.CreateQuery("select c from Customer c where c.FirstName like 'J%'"); 
                // var customers = session.CreateQuery("select c from Customer c where size(c.Orders) > 9");     
                // var customers = session.CreateQuery("select c from Customer c where c.Orders.size > 9");     

                foreach (var customer in customers.List<Customer>())
                {
                    Console.WriteLine(customer);
                }

                tx.Commit();
            }
            */

            /* Query Comprehension Syntax demonstration
            using (var tx = session.BeginTransaction())
            {
                var customers = from c in session.Query<Customer>()
                                where c.FirstName.StartsWith("J")
                                select c;

                foreach (var customer in customers.ToList())
                {
                    Console.WriteLine(customer);
                }
                tx.Commit();
            }
            */

            /* Linq demonstration
            using (var tx = session.BeginTransaction())
            {
                var customer = session.Query<Customer>().Where(c => c.FirstName == "John").First();
                Console.WriteLine(customer);
                tx.Commit();
            }
            */

            /* Load/Get demonstration
            using (var tx = session.BeginTransaction())
            {
                var id1 = Guid.Parse("d7af964e-a733-4d7c-8111-ab47007a8df6");
                var id2 = Guid.Parse("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE");

                var customer1 = session.Load<Customer>(id1);
                Console.WriteLine("Customer1 data");
                Console.WriteLine(customer1);

                var customer2 = session.Load<Customer>(id2);
                Console.WriteLine("Customer2 data");
                Console.WriteLine(customer2);

                var customer1 = session.Get<Customer>(id1);
                Console.WriteLine("Customer1 data");
                Console.WriteLine(customer1);

                var customer2 = session.Get<Customer>(id2);
                Console.WriteLine("Customer2 data");
                Console.WriteLine(customer2);

                tx.Commit();
            }
            */

            /*
            Guid id;
            // transaction to create customers and orders
            using (var tx = session.BeginTransaction())
            {
                var newCustomer = CreateCustomer();
                Console.WriteLine("New Customer:");
                Console.WriteLine(newCustomer);
                session.Save(newCustomer);

                // Manually having to save Orders for the customer here 
                foreach (var order in newCustomer.Orders)
                {
                    session.Save(order);
                }


                id = newCustomer.Id;
                tx.Commit();
            }
            */

            // using (var session = sessionFactory.OpenSession())

            /*
            // transaction to bring up (load) existing customers and orders
            using (var tx = session.BeginTransaction())
            {
                // join and fetch happening at once at query level 
                var query = from customer in session.Query<Customer>()
                            where customer.Id == id
                            select customer;
                var reloaded = query.Fetch(x => x.Orders).ToList().First();

                //var reloaded = session.Load<Customer>(id);
                Console.WriteLine("Reloaded:");
                Console.WriteLine(reloaded);


                Console.WriteLine("The orders were ordered by: ");

                foreach (var order in reloaded.Orders)
                {
                    Console.WriteLine(order.Customer);
                }


                tx.Commit();
            }
            */

            Console.WriteLine("Press <ENTER> to exit...");
            Console.ReadLine();

        }
    

        /*
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
        */

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
