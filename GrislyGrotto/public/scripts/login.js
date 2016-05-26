function select(id) {
	return document.getElementById(id);
}

select('submit').onclick = function (e) {
	var username = select('username').value.toLowerCase().trim();
	var password = select('password').value.toLowerCase().trim();

	if (!username || !password) {
		var error = select('error');
		error.innerText = 'Both username and password must be specified';
		error.setAttribute('style', 'display: both');
		e.preventDefault();
		return false;
	}

	return true;
};