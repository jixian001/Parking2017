namespace Parking.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addtlog_plate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TelegramLogs", "PlateNum", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TelegramLogs", "PlateNum");
        }
    }
}
