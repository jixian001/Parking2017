namespace Parking.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatecc : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FixUserChargeLogs", "OprtCode", c => c.String());
            AddColumn("dbo.FixUserChargeLogs", "RecordDTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.TempUserChargeLogs", "RecordDTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TempUserChargeLogs", "RecordDTime", c => c.String());
            DropColumn("dbo.FixUserChargeLogs", "RecordDTime");
            DropColumn("dbo.FixUserChargeLogs", "OprtCode");
        }
    }
}
