namespace Parking.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addplate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WorkTasks", "PlateNum", c => c.String(maxLength: 10));
        }
        
        public override void Down()
        {
            DropColumn("dbo.WorkTasks", "PlateNum");
        }
    }
}
