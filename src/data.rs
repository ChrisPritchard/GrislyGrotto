use sqlite::{Value, State};

use crate::model::BlogPost;

pub async fn get_latest_posts(page: i64, current_user: String) -> Result<Vec<BlogPost>, sqlite::Error> {

    let connection = sqlite::open("./grislygrotto.db").unwrap();
    let skip = page * 5;
    let query = "
        SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Author_Username, p.Key, p.Title, p.Content, p.Date, p.IsStory, p.WordCount,
			(SELECT COUNT(*) FROM Comments WHERE Post_Key = p.Key) as CommentCount
		FROM Posts p
		WHERE 
			(p.Title NOT LIKE '[DRAFT] %' OR p.Author_Username = ?)
		ORDER BY p.Date DESC 
		LIMIT ? OFFSET ?
        ";
	let mut statement = connection.prepare(query)?;
	statement.bind::<&[(_, Value)]>(&[
		(1, current_user.into()), 
		(2, 5.into()), 
		(3, skip.into())])?;

	let mut final_result = Vec::new();
	
	let mut markdown_options = comrak::ComrakOptions::default();
	markdown_options.render.unsafe_ = true;

	while let Ok(State::Row) = statement.next() {
		let content = statement.read::<String, _>("Content").unwrap();
		final_result.push(BlogPost { 
			author: statement.read::<String, _>("Author").unwrap(), 
			author_username: statement.read::<String, _>("Author_Username").unwrap(), 
			key: statement.read::<String, _>("Key").unwrap(), 
			title: statement.read::<String, _>("Title").unwrap(), 
			content: comrak::markdown_to_html(&content, &markdown_options),
			date: statement.read::<String, _>("Date").unwrap(), 
			is_story: statement.read::<i64, _>("IsStory").unwrap() > 0, 
			word_count: statement.read::<i64, _>("WordCount").unwrap(), 
			comment_count: statement.read::<i64, _>("CommentCount").unwrap() })
	}
    
    Ok(final_result)
}