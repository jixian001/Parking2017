using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Parking.Data
{
    public class ParkingContext:DbContext
    {
        public ParkingContext() : 
            base("DefaultConnection")
        {
            Database.SetInitializer<ParkingContext>(new CreateDatabaseIfNotExists<ParkingContext>());
        }
        //添加各表的Dbset<TEntity>
        public DbSet<Alarm> Alarms { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<ICCard> ICCards { get; set; }
        public DbSet<ImplementTask> ImplementTasks { get; set; }
        public DbSet<Location> Locactions { get; set; }
        public DbSet<WorkTask> WorkTasks { get; set; }
        public DbSet<FaultLog> FautLogs { get; set; }
        public DbSet<OperateLog> OperateLog { get; set; }
        public DbSet<TelegramLog> TelegramLogs { get; set; }

    }
}
