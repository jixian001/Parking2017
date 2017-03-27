namespace Parking.Web.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Parking.Data;

    internal sealed class Configuration : DbMigrationsConfiguration<Parking.Data.ParkingContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Parking.Data.ParkingContext";
        }

        protected override void Seed(Parking.Data.ParkingContext context)
        {
           
        }
    }
}
