var sql = require('node-sqlserver-unofficial');

var connectionString = 'Driver={SQL Server Native Client 11.0};Server=tcp:mhvrgafbmv.database.windows.net,1433;Database=GrislyGrottoDB_v12.8;Uid=grislygrotto_dbuser@mhvrgafbmv;Pwd=***REMOVED***;Encrypt=yes;Connection Timeout=30;';

sql.query(connectionString, "SELECT * FROM Users", function (err, results) {

});

console.log('finished');