
var data = require('./data');
var util = require('./util');

exports.configure = function(app) {

	createInvokeAndModelMethods();
	
	app.get('/', function(req, res) {
		data.latest.invoke(res, function(posts) {
			res.rendermodel('latest', 'Latest', req, {
				posts: posts,
				onLatest: true
			});
		});
	});
	
	app.get('/p/:key', function(req, res) {
		data.single.invoke(res, req.params.key, function(post) {
			res.rendermodel('single', post.Title, req, { post: post });
		});
	});
	
	app.post('/p/:key', function (req, res) {
		var key = req.params.key;
		if (!util.validateCommentFromBody(req)) {
			res.send(400);
			return;
		}
		data.newComment.invoke(res, { Post_ID: req.body.postID, Author: req.body.author.trim(), Content: req.body.content.trim() }, function () {
			res.redirect('/p/' + key + '#comments');
		});
	});
	
	app.get('/stories', function(req, res) {
		data.stories.invoke(res, function(stories) {
			res.rendermodel('stories', 'Stories', req, {
				posts: stories,
				onStories: true
			});
		});
	});
	
	app.get('/archives', function(req, res){
		data.archives.invoke(res, function(years) {
			res.rendermodel('archives', 'Archives', req, {
				years: years,
				onArchives: true
			});
		});
	});
	
	app.get('/m/:name/:year', function(req, res) {
		data.month.invoke(res, req.params.name, req.params.year, function(posts) {
			res.rendermodel('month', req.params.name + ', ' + req.params.year, req, { posts: posts });
		});
	});

	app.get('/search', function (req, res) {
		res.render('search', {
			title: 'Search',
			posts: [],
			onSearch: true
		});
	});

	app.post('/search', function (req, res) {
		if ((req.body.searchTerm || '').trim().length == 0) {
			res.send(400);
			return;
		}
		data.search.invoke(res, req.body.searchTerm, function (posts) {
			res.rendermodel('search', 'Search', req, {
				posts: posts,
				onSearch: true
			});
		});
	});
	
	app.get('/login', function(req, res) {
		res.render('login', {
			title: 'Login',
			onLogin: true
		});
	});
	
	app.post('/login', function (req, res) {
		if (!((req.body.username || '').trim()) || !((req.body.username || '').trim())) {
			res.send(400);
			return;
		}
		data.validateUser.invoke(res, req.body.username.trim(), req.body.password.trim(),
			function (user) {
				if (user) {
					req.session.loggedin = user;
					res.redirect('/');
				}
				else
					res.render('login', {
						title: 'Login',
						failed: true,
						onLogin: true
					});
			});
		
	});

	app.get('/checkid/:key', function(req, res) {
		data.existsForId(req.params.key, function(exists) {
			res.send(!exists);
		});
	});
	
	app.get('/new', function(req, res) {
		if (!req.session.loggedin) {
			res.send(401);
			return;
		}
		res.render('editor', {
			title: 'Edit Post',
			onEditor: true
		});
	});
	
	app.post('/new', function(req, res){
		if (!req.session.loggedin) {
			res.send(401);
			return;
		}
		else if (!util.validatePostFromBody(req)) {
			res.send(400);
			return;
		}
		var post = util.postFromBody(req);
		post.Author_ID = req.session.loggedin.ID;
		data.create.invoke(res, post, function() {
			res.redirect('/p/' + post.Key);
		});
	});
	
	app.get('/edit/:key', function(req, res) {
		if (!req.session.loggedin) {
			res.send(401);
			return;
		}
		data.single.invoke(res, req.params.key, function(post) {
			if (post.Username != req.session.loggedin.Username)
				res.send(401);
			else 
				res.render('editor', {
					title: 'Edit Post',
					post: post,
					onEditor: true
				});
		});
	});
	
	app.post('/edit/:key', function(req, res){
		if (!req.session.loggedin) {
			res.send(401);
			return;
		}
		if (!util.validatePostFromBody(req)) {
			res.send(400);
			return;
		}
		data.single.invoke(res, req.params.key, function (existingPost) {
			if (existingPost.Username != req.session.loggedin.Username)
				res.send(401);
			else {
				var post = util.postFromBody(req);
				data.update.invoke(res, existingPost.ID, post, function() {
					res.redirect('/p/' + post.Key);
				});
			}
		});
	});
};

function createInvokeAndModelMethods() {
	Function.prototype.invoke = function(res, args1OrCallback, args2OrCallback, callback) {
		function handleCallback(error, data, subcallback) {
			if (error)
				res.send(500, { error: JSON.stringify(error) });
			else
				subcallback(data);
		};

		res.rendermodel = function(view, title, req, additionalElements) {
			var sessionLoggedIn = req.session.loggedin;
			var baseModel = {
				title: title,
				loggedIn: sessionLoggedIn
			};
			if (sessionLoggedIn && additionalElements.post)
				baseModel.loggedInAsAuthor = sessionLoggedIn.Username == additionalElements.post.Username;
			if(additionalElements)
				for (var i in additionalElements)
					baseModel[i] = additionalElements[i];

			res.render(view, baseModel);
		};
		
		if(callback)
			this(args1OrCallback, args2OrCallback, function(error, data) { handleCallback(error, data, callback); });
		else if(args2OrCallback)
			this(args1OrCallback, function(error, data) { handleCallback(error, data, args2OrCallback); });
		else
			this(function(error, data) { handleCallback(error, data, args1OrCallback); });
	};
}