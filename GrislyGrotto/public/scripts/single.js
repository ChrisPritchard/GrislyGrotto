function select(id) {
	return document.getElementById(id);
}

select('submit').onclick = function (e) {
	var author = select('author').value.toLowerCase().trim();
	var content = select('content').value.toLowerCase().trim();

	var errorMessage = '';
	if ((!author || author.length > 100) || (!content || content.length > 500))
		errorMessage = 'Author and Content must be specified, and be under 100 and 500 characters respectively';
	else if (content.indexOf('href') >= 0 || content.indexOf('http') >= 0 || content.indexOf('<') >= 0)
		errorMessage = 'I\'m sorry, comments cannot contain HTML or HTML-like content (damn spammers)';

	if (errorMessage) {
		var error = select('error');
		error.innerText = errorMessage;
		error.setAttribute('style', 'display: both');
		e.preventDefault();
		return false;
	}

	select('submit').setAttribute('disabled', 'disabled');
	return true;
};