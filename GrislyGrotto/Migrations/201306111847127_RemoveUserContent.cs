namespace GrislyGrotto.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUserContent : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.UserContents");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UserContents",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Key = c.String(),
                        Data = c.Binary(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
    }
}
