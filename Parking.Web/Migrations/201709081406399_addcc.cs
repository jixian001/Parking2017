namespace Parking.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addcc : DbMigration
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
            
        }
        
        public override void Down()
        {
            DropTable("dbo.StatusInfoLogs");
            DropTable("dbo.DeviceInfoLogs");
        }
    }
}
