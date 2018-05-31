module Data
open FSharp.Data.Sql

let [<Literal>]ConnString = "Server=tcp:grislygrotto.database.windows.net,1433;Data Source=grislygrotto.database.windows.net;Initial Catalog=grislygrotto;Persist Security Info=False;User ID=grislygrotto_user;Password=***REMOVED***;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

type GrislyData = 
    SqlDataProvider<
        Common.DatabaseProviderTypes.MSSQLSERVER, 
        ConnString, 
        IndividualsAmount = 0, 
        CaseSensitivityChange = Common.CaseSensitivityChange.ORIGINAL,
        UseOptionTypes = true>

let test = GrislyData.GetDataContext() 

let latest = 
    query {
        for post in test.Dbo.Posts do
            sortBy post.Date
            take 5
            select post
    } |> Seq.toList