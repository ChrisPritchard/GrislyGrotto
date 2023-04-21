
/// provide: username, count, skip
pub const SELECT_LATEST_POSTS: &str = "
	SELECT 
		(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
		p.Author_Username, p.Key, p.Title, p.Content, p.Date, p.IsStory, p.WordCount,
		(SELECT COUNT(*) FROM Comments WHERE Post_Key = p.Key) as CommentCount
	FROM Posts p
	WHERE 
		(p.Title NOT LIKE '[DRAFT] %' OR p.Author_Username = ?)
	ORDER BY p.Date DESC 
	LIMIT ? OFFSET ?";

/// provide: key, username
pub const SELECT_SINGLE_POST: &str = "
	SELECT 
		(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
		p.Author_Username, p.Title, p.Content, p.Date, p.IsStory
	FROM Posts p
	WHERE 
		Key = ?
		AND (p.Title NOT LIKE '[DRAFT] %' OR p.Author_Username = ?)";

/// provide: post_key
pub const SELECT_COMMENTS: &str = "
	SELECT 
		Id, Author, Date, Content
	FROM Comments
	WHERE Post_Key = ?
	ORDER BY Date";