use sqlite::Statement;

use crate::model::*;

pub fn post_from_statement(stmt: &Statement, markdown_options: &comrak::ComrakOptions) -> Result<BlogPost, sqlite::Error> {
    let content: String = stmt.read("Content")?;
    let is_story: i64 = stmt.read("IsStory")?;
    
    Ok(BlogPost { 
        author: stmt.read("Author")?, 
        author_username: stmt.read("Author_Username")?, 
        key: stmt.read("Key")?, 
        title: stmt.read("Title")?, 
        content: comrak::markdown_to_html(&content, markdown_options),
        date: stmt.read("Date")?, 
        is_story: is_story > 0, 
        word_count: stmt.read("WordCount")?, 
        comment_count: stmt.read("CommentCount")? })
}