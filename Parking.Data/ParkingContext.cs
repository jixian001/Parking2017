using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Parking.Data
{
    public class ParkingContext:DbContext,IDisposable
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
        public DbSet<OperateLog> OperateLogs { get; set; }
        public DbSet<TelegramLog> TelegramLogs { get; set; }
        public DbSet<PlateMappingDev> PlateMappingDevs { get; set; }
        //收费相关
        public DbSet<FixChargingRule> FixChargingRules { get; set; }
        public DbSet<PreCharging> PreChargings { get; set;}
        public DbSet<TempChargingRule> TempChargingRules { get; set; }
        public DbSet<OrderChargeDetail> OrderChargeDetails { get; set; }
        public DbSet<HourChargeDetail> HourChargeDetails { get; set; }
        public DbSet<HourSectionInfo> HourSectionInfoes { get; set; }
        public DbSet<FingerPrint> FingerPrints { get; set; }
        public DbSet<TempUserChargeLog> TempUserChargeLogs { get; set; }
        public DbSet<FixUserChargeLog> FixUserChargeLogs { get; set; }

        

    }
}
