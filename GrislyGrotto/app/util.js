
var months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
exports.months = months;

function pad(obj) {
	var val = obj.toString();
	if (val.length == 1)
		val = '0' + val;
	return val;
}

function getNzDate() {
  var date = new Date();
  var localTime = date.getTime();
  var utc = localTime + (date.getTimezoneOffset() * 60000);

  var nzOffset = 12 * 60 * 60000;
  var nzTime = utc + nzOffset;

  return new Date(nzTime);
};
exports.getNzDate = getNzDate;

exports.validateCommentFromBody = function (req) {
	var author = (req.body.author || '').trim();
	var content = (req.body.content || '').trim().toLowerCase();
	return (author && content && author.length < 100 && content.length < 500
		&& !(content.indexOf('href') >= 0 || content.indexOf('http') >= 0 || content.indexOf('<') >= 0));
};

exports.validatePostFromBody = function (req) {
	var title = (req.body.title || '').trim();
	var content = (req.body.content || '').trim();
	return title && title.length <= 100 && content && content.length > 500;
};

exports.postFromBody = function (req) {
	return {
		Title: req.body.title,
		Key: req.body.title.replace(/ /g, '-').replace(/[^A-Za-z0-9 -]+/g, '').toLowerCase(),
		Content: req.body.content,
		IsStory: req.body.isstory,
		WordCount: req.body.content.replace(/(<([^>]+)>)/ig, '').split(' ').length
	};
};