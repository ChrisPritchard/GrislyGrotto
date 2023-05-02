
/// provide: current_user, count, skip
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

/// provide: key, current_user
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

/// provide: post_key
pub const SELECT_COMMENT_COUNT: &str = "
	SELECT 
		Count(Id) AS Count
	FROM Comments
	WHERE Post_Key = ?";

/// provide: author, date, content, post_key
pub const INSERT_COMMENT: &str = "
    INSERT INTO 
        Comments (Author, Date, Content, Post_Key) 
    VALUES (?, ?, ?, ?)";

pub const GET_LAST_COMMENT_ID: &str = "
	SELECT Id FROM Comments ORDER BY Id DESC LIMIT 1";

/// provide: id
pub const SELECT_COMMENT_CONTENT: &str = "
	SELECT Content FROM Comments WHERE Id = ?";

/// provide: content, id
pub const UPDATE_COMMENT_CONTENT: &str = "
	UPDATE Comments SET Content = ? WHERE Id = ?";

/// provide: id
pub const DELETE_COMMENT: &str = "
	DELETE FROM Comments WHERE Id = ?";

/// provide: current_user
pub const SELECT_MONTH_COUNTS: &str = "
	SELECT 
		SUBSTR(Date, 0, 8) as Month, COUNT(Key) as Count 
	FROM 
		Posts 
	WHERE
		(Title NOT LIKE '[DRAFT] %' OR Author_Username = ?)
	GROUP BY 
		Month 
	ORDER BY 
		Date DESC";

/// provide: current_user
pub const SELECT_STORIES: &str = "
	SELECT 
		(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
		p.Key, p.Title, p.Date, p.IsStory, p.WordCount
	FROM Posts p 
	WHERE 
		p.IsStory = 1
		AND (p.Title NOT LIKE '[DRAFT] %' OR p.Author_Username = ?)
	ORDER BY p.Date DESC";

/// provide: 'YYYY-MM', current_user
pub const SELECT_MONTH_POSTS: &str = "
	SELECT 
		(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
		p.Author_Username, p.Key, p.Title, p.Content, p.Date, p.IsStory, p.WordCount, 
		(SELECT COUNT(*) FROM Comments WHERE Post_Key = p.Key) as CommentCount
	FROM Posts p
	WHERE 
		SUBSTR(p.Date, 0, 8) = ?
		AND (p.Title NOT LIKE '[DRAFT] %' OR p.Author_Username = ?)
	ORDER BY p.Date";

/// provide: search_term, search_term, current_user
pub const SELECT_SEARCH_RESULTS: &str = "
	SELECT 
		(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
		p.Key, p.Title, p.Content, p.Date
	FROM Posts p
	WHERE 
		(p.Title LIKE ? OR p.Content LIKE ?)
		AND (p.Title NOT LIKE '[DRAFT] %' OR p.Author_Username = ?)
	ORDER BY p.Date DESC 
	LIMIT 50";

/// provide: username
pub const SELECT_USER_PASSWORD_HASH: &str = "
	SELECT 
		Password
	FROM Authors
	WHERE Username = ?";

/// provide: new_password_hash, username
pub const UPDATE_USER_PASSWORD_HASH: &str = "
	UPDATE Authors 
	SET Password = ? 
	WHERE Username = ?";

/// provide: username
pub const SELECT_USER_DISPLAY_NAME: &str = "
	SELECT 
		DisplayName
	FROM Authors
	WHERE Username = ?";

/// provide: new_display_name, username
pub const UPDATE_USER_DISPLAY_NAME: &str = "
	UPDATE Authors 
	SET DisplayName = ? 
	WHERE Username = ?";