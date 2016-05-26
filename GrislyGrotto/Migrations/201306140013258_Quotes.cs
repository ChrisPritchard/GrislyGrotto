namespace GrislyGrotto.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quotes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Quotes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Text = c.String(),
                        Author = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Quotes");
        }
    }
}
