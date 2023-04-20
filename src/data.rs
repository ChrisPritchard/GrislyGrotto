use sqlite::{Value, State};

use crate::model::BlogPost;

/// provide params as: username, count, skip
const QUERY_LATEST_POSTS: &str = "
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Author_Username, p.Key, p.Title, p.Content, p.Date, p.IsStory, p.WordCount,
			(SELECT COUNT(*) FROM Comments WHERE Post_Key = p.Key) as CommentCount
		FROM Posts p
		WHERE 
			(p.Title NOT LIKE '[DRAFT] %' OR p.Author_Username = ?)
		ORDER BY p.Date DESC 
		LIMIT ? OFFSET ?";

pub async fn get_latest_posts(page: i64, current_user: String) -> Result<Vec<BlogPost>, sqlite::Error> {

    let connection = sqlite::open("./grislygrotto.db")?;

	let mut stmt = connection.prepare(QUERY_LATEST_POSTS)?;
	stmt.bind::<&[(_, Value)]>(&[
		(1, current_user.into()), 
		(2, 5.into()), 
		(3, (page * 5).into())])?;

	let mut final_result = Vec::new();
	
	let mut markdown_options = comrak::ComrakOptions::default();
	markdown_options.render.unsafe_ = true;

	while let Ok(State::Row) = stmt.next() {
		let content: String = stmt.read("Content")?;
		let is_story: i64 = stmt.read("IsStory")?;
		final_result.push(BlogPost { 
			author: stmt.read("Author")?, 
			author_username: stmt.read("Author_Username")?, 
			key: stmt.read("Key")?, 
			title: stmt.read("Title")?, 
			content: comrak::markdown_to_html(&content, &markdown_options),
			date: stmt.read("Date")?, 
			is_story: is_story > 0, 
			word_count: stmt.read("WordCount")?, 
			comment_count: stmt.read("CommentCount")? })
	}
    
    Ok(final_result)
}