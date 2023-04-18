use actix_web::{web::Data};
use sqlx::{SqlitePool, query_as};

use crate::model::BlogPost;

pub async fn get_latest_posts(db: Data<SqlitePool>, page: i32, current_user: Option<String>) -> Result<Vec<BlogPost>, sqlx::Error> {

    let mut pool = db.acquire().await.unwrap();
    let skip = page * 5;
    let result = query_as!(BlogPost, "
        SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as 'author!',
			p.Author_Username as 'author_username!', 
			p.Key as 'key!', 
			p.Title as 'title!', 
			p.Content as 'content!', 
			p.Date as 'date!', 
			p.IsStory as 'is_story!', 
			p.WordCount as 'word_count!',
			(SELECT COUNT(*) FROM Comments WHERE Post_Key = p.Key) as 'comment_count!'
		FROM Posts p
		WHERE 
			(p.Title NOT LIKE '[DRAFT] %' OR p.Author_Username = ?)
		ORDER BY p.Date DESC 
		LIMIT ? OFFSET ?
        ", current_user, 5, skip).fetch_all(&mut pool).await?;
    
    Ok(result)
}