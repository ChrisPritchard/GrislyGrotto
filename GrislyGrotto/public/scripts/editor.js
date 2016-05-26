
function select(id) {
	return document.getElementById(id);
}
function selectAll(name) {
	return document.getElementsByName(name);
}

function display(idOrElement, mode) {
	if (idOrElement.nodeName)
		idOrElement.setAttribute('style', 'display:' + mode);
	else
		select(idOrElement).setAttribute('style', 'display:' + mode);
}

function create(elementName, attributes) {
	var newElement = document.createElement(elementName);
	for (var name in attributes)
		newElement.setAttribute(name, attributes[name]);
	return newElement;
}

function ajax(url, callback) {
	var request;
	if (window.XMLHttpRequest)
		request = new XMLHttpRequest();
	else
		request = new ActiveXObject("Microsoft.XMLHTTP");
	
	request.open('GET', url, true);
	request.setRequestHeader('Content-Type', 'text/x-json');
	request.onreadystatechange = function() {
		if (request.readyState == 4 && callback)
			callback(JSON.parse(request.responseText));
	};
	request.send();
}

// view type select
select('normal').onclick = select('html').onclick = function() {
	var content = select('content');
	if (this.id == 'normal')
		content.innerHTML = content.innerText;
	else
		content.innerText = content.innerHTML;
};

// validation, content extraction prior to submit
select('submitbutton').onclick = function() {
	var contentDiv = select('content');
	var content = contentDiv.innerHTML;
	if (select('html').checked)
		content = contentDiv.innerText;
	
	var title = select('title');
	if (!title.value.length || title.value.length > 100 || content.length < 500) {
		select('error').innerText = 'You must have a title specified that is less than 100 characters and a total content length of at least 500 characters.';
		display('error', 'inline');
		return false;
	}

	if (title.value == select('originalTitle').value) {
		sendUpdate(content);
		return false;
	}

	select('submitbutton').setAttribute('disabled', 'disabled');

	var possibleId = title.value.replace(/ /g, '-').replace(/[^A-Za-z0-9 -]+/g, '').toLowerCase();
	ajax('/checkid/' + possibleId, function(unique) {
		if (!unique) {
			select('error').innerHTML = 'This title results in an id that is in use (link: <a href="/p/' + possibleId + '">' + possibleId + '</a>).';
			display('error', 'inline');
			select('submitbutton').removeAttribute('disabled');
		} else 
			sendUpdate(content);
	});
	
	return false;
};

function sendUpdate(content) {
	var newHidden = create('input', {
		type: 'hidden',
		name: 'content',
		value: content
	});
	document.forms[0].appendChild(newHidden);
	document.forms[0].submit();
}