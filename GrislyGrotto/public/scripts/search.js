function select(id) {
	return document.getElementById(id);
}

select('submit').onclick = function (e) {
	var searchTerm = select('searchTerm').value.toLowerCase().trim();

	if (!searchTerm) {
		var error = select('error');
		error.innerText = 'Search term must be specified';
		error.setAttribute('style', 'display: both');
		e.preventDefault();
		return false;
	}

	return true;
};