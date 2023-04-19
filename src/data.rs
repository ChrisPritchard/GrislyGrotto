use actix_web::{web::Data};
use sqlx::{query, SqlitePool};

use crate::model::BlogPost;

pub async fn get_latest_posts(db: Data<SqlitePool>, page: i32, current_user: Option<String>) -> Result<Vec<BlogPost>, sqlx::Error> {

    let mut pool = db.acquire().await.unwrap();
    let skip = page * 5;
    let query_result = query!("
        SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Author_Username, p.Key, p.Title, p.Content, p.Date, p.IsStory, p.WordCount,
			(SELECT COUNT(*) FROM Comments WHERE Post_Key = p.Key) as CommentCount
		FROM Posts p
		WHERE 
			(p.Title NOT LIKE '[DRAFT] %' OR p.Author_Username = ?)
		ORDER BY p.Date DESC 
		LIMIT ? OFFSET ?
        ", current_user, 5, skip).fetch_all(&mut pool).await?;

	let mut final_result = Vec::new();
	
	let mut markdown_options = comrak::ComrakOptions::default();
	markdown_options.render.unsafe_ = true;

	for entry in query_result {
		final_result.push(BlogPost { 
			author: entry.Author.unwrap(), 
			author_username: entry.Author_Username.unwrap(), 
			key: entry.Key.unwrap(), 
			title: entry.Title.unwrap(), 
			content: comrak::markdown_to_html(&entry.Content.unwrap(), &markdown_options),
			date: entry.Date.unwrap(), 
			is_story: entry.IsStory.unwrap() > 0, 
			word_count: entry.WordCount.unwrap(), 
			comment_count: entry.CommentCount.unwrap() })
	}
    
    Ok(final_result)
}