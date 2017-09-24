namespace Parking.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DeviceInfoLogs",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Warehouse = c.Int(nullable: false),
                        DeviceCode = c.Int(nullable: false),
                        RecordDtime = c.DateTime(nullable: false),
                        Mode = c.String(),
                        IsAble = c.Int(nullable: false),
                        IsAvailabe = c.Int(nullable: false),
                        RunStep = c.Int(nullable: false),
                        InStep = c.Int(nullable: false),
                        OutStep = c.Int(nullable: false),
                        Address = c.String(),
                        TaskID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.StatusInfoLogs",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Warehouse = c.Int(nullable: false),
                        DeviceCode = c.Int(nullable: false),
                        Description = c.String(),
                        RcdDtime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.FixUserChargeLogs", "OprtCode", c => c.String());
            AddColumn("dbo.FixUserChargeLogs", "RecordDTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.WorkTasks", "PlateNum", c => c.String(maxLength: 10));
            AlterColumn("dbo.TelegramLogs", "ICCode", c => c.String());
            AlterColumn("dbo.TempUserChargeLogs", "RecordDTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TempUserChargeLogs", "RecordDTime", c => c.String());
            AlterColumn("dbo.TelegramLogs", "ICCode", c => c.String(maxLength: 10));
            DropColumn("dbo.WorkTasks", "PlateNum");
            DropColumn("dbo.FixUserChargeLogs", "RecordDTime");
            DropColumn("dbo.FixUserChargeLogs", "OprtCode");
            DropTable("dbo.StatusInfoLogs");
            DropTable("dbo.DeviceInfoLogs");
        }
    }
}
