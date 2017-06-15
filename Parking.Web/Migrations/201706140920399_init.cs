namespace Parking.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Alarms",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Address = c.Int(nullable: false),
                        Description = c.String(),
                        Value = c.Byte(nullable: false),
                        Color = c.Int(nullable: false),
                        IsBackup = c.Byte(nullable: false),
                        Warehouse = c.Int(nullable: false),
                        DeviceCode = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserName = c.String(maxLength: 20),
                        PlateNum = c.String(nullable: false, maxLength: 20),
                        FamilyAddress = c.String(),
                        MobilePhone = c.String(maxLength: 25),
                        Telephone = c.String(maxLength: 25),
                        HeadShot = c.Binary(),
                        ImagePath = c.String(),
                        ImageData = c.Binary(),
                        Type = c.Int(nullable: false),
                        StartDTime = c.DateTime(nullable: false),
                        Deadline = c.DateTime(nullable: false),
                        Warehouse = c.Int(nullable: false),
                        LocAddress = c.String(maxLength: 10),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Devices",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Warehouse = c.Int(nullable: false),
                        DeviceCode = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        HallType = c.Int(nullable: false),
                        Address = c.String(maxLength: 10),
                        Layer = c.Int(nullable: false),
                        Region = c.Int(nullable: false),
                        Mode = c.Int(nullable: false),
                        IsAble = c.Int(nullable: false),
                        IsAvailabe = c.Int(nullable: false),
                        RunStep = c.Int(nullable: false),
                        InStep = c.Int(nullable: false),
                        OutStep = c.Int(nullable: false),
                        TaskID = c.Int(nullable: false),
                        SoonTaskID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.FaultLogs",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Warehouse = c.Int(nullable: false),
                        DeviceCode = c.Int(nullable: false),
                        RunStep = c.Int(nullable: false),
                        InStep = c.Int(nullable: false),
                        OutStep = c.Int(nullable: false),
                        Description = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.FingerPrints",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        SN_Number = c.Short(nullable: false),
                        FingerInfo = c.String(nullable: false),
                        CustID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.FixChargingRules",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ICType = c.Int(nullable: false),
                        Unit = c.Int(nullable: false),
                        Fee = c.Single(nullable: false),
                        PreChgID = c.Int(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.FixUserChargeLogs",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                        PlateNum = c.String(),
                        UserType = c.String(),
                        Proof = c.String(),
                        LastDeadline = c.String(),
                        CurrDeadline = c.String(),
                        FeeType = c.String(),
                        FeeUnit = c.Single(nullable: false),
                        CurrFee = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.HourChargeDetails",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TempChgID = c.Int(nullable: false),
                        CycleTime = c.Int(nullable: false),
                        StrideDay = c.Int(nullable: false),
                        CycleTopFee = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.HourSectionInfoes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        HourChgID = c.Int(nullable: false),
                        StartTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                        SectionTopFee = c.Single(nullable: false),
                        SectionFreeTime = c.String(),
                        FirstVoidTime = c.String(),
                        FirstVoidFee = c.Single(nullable: false),
                        IntervalVoidTime = c.String(),
                        IntervalVoidFee = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.ICCards",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PhysicCode = c.String(maxLength: 20),
                        UserCode = c.String(nullable: false, maxLength: 4),
                        Status = c.Int(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                        LossDate = c.DateTime(nullable: false),
                        LogoutDate = c.DateTime(nullable: false),
                        CustID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.ImplementTasks",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Warehouse = c.Int(nullable: false),
                        DeviceCode = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                        SendDtime = c.DateTime(nullable: false),
                        HallCode = c.Int(nullable: false),
                        FromLctAddress = c.String(maxLength: 10),
                        ToLctAddress = c.String(maxLength: 10),
                        ICCardCode = c.String(maxLength: 10),
                        Distance = c.Int(nullable: false),
                        CarSize = c.String(),
                        CarWeight = c.Int(nullable: false),
                        IsComplete = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Locations",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Warehouse = c.Int(nullable: false),
                        Address = c.String(),
                        LocSide = c.Int(nullable: false),
                        LocColumn = c.Int(nullable: false),
                        LocLayer = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        LocSize = c.String(maxLength: 10),
                        Region = c.Int(nullable: false),
                        NeedBackup = c.Int(nullable: false),
                        Index = c.Int(nullable: false),
                        ICCode = c.String(maxLength: 10),
                        WheelBase = c.Int(nullable: false),
                        CarSize = c.String(maxLength: 10),
                        InDate = c.DateTime(nullable: false),
                        PlateNum = c.String(),
                        ImagePath = c.String(),
                        ImageData = c.Binary(),
                        CarWeight = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.OperateLogs",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                        OptName = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.OrderChargeDetails",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TempChgID = c.Int(nullable: false),
                        OrderFreeTime = c.String(),
                        Fee = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.PlateMappingDevs",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Warehouse = c.Int(nullable: false),
                        DeviceCode = c.Int(nullable: false),
                        PlateNum = c.String(),
                        HeadImagePath = c.String(),
                        PlateImagePath = c.String(),
                        InDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.PreChargings",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CycleNum = c.Int(nullable: false),
                        CycleUnit = c.Int(nullable: false),
                        Fee = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.TelegramLogs",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        RecordDtime = c.DateTime(nullable: false),
                        Type = c.Int(nullable: false),
                        Warehouse = c.Int(nullable: false),
                        Telegram = c.String(),
                        DeviceCode = c.Int(nullable: false),
                        ICCode = c.String(maxLength: 10),
                        CarInfo = c.String(),
                        FromAddress = c.String(maxLength: 10),
                        ToAddress = c.String(maxLength: 10),
                        TelegramID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.TempChargingRules",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ICType = c.Int(nullable: false),
                        TempChgType = c.Int(nullable: false),
                        PreChgID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.TempUserChargeLogs",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Proof = c.String(),
                        Plate = c.String(),
                        Warehouse = c.Int(nullable: false),
                        Address = c.String(),
                        InDate = c.String(),
                        OutDate = c.String(),
                        SpanTime = c.String(),
                        NeedFee = c.String(),
                        ActualFee = c.String(),
                        CoinChange = c.String(),
                        OprtCode = c.String(),
                        RecordDTime = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.WorkTasks",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        IsMaster = c.Int(nullable: false),
                        Warehouse = c.Int(nullable: false),
                        DeviceCode = c.Int(nullable: false),
                        MasterType = c.Int(nullable: false),
                        TelegramType = c.Int(nullable: false),
                        SubTelegramType = c.Int(nullable: false),
                        HallCode = c.Int(nullable: false),
                        FromLctAddress = c.String(maxLength: 10),
                        ToLctAddress = c.String(maxLength: 10),
                        ICCardCode = c.String(maxLength: 10),
                        Distance = c.Int(nullable: false),
                        CarSize = c.String(maxLength: 10),
                        CarWeight = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.WorkTasks");
            DropTable("dbo.TempUserChargeLogs");
            DropTable("dbo.TempChargingRules");
            DropTable("dbo.TelegramLogs");
            DropTable("dbo.PreChargings");
            DropTable("dbo.PlateMappingDevs");
            DropTable("dbo.OrderChargeDetails");
            DropTable("dbo.OperateLogs");
            DropTable("dbo.Locations");
            DropTable("dbo.ImplementTasks");
            DropTable("dbo.ICCards");
            DropTable("dbo.HourSectionInfoes");
            DropTable("dbo.HourChargeDetails");
            DropTable("dbo.FixUserChargeLogs");
            DropTable("dbo.FixChargingRules");
            DropTable("dbo.FingerPrints");
            DropTable("dbo.FaultLogs");
            DropTable("dbo.Devices");
            DropTable("dbo.Customers");
            DropTable("dbo.Alarms");
        }
    }
}
