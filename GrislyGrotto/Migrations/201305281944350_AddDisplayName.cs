namespace GrislyGrotto.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDisplayName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "DisplayName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "DisplayName");
        }
    }
}
