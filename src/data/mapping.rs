use sqlite::Statement;

use crate::model::*;

pub fn post_from_statement(stmt: &Statement, markdown_options: &comrak::ComrakOptions) -> Result<BlogPost, Box<dyn std::error::Error>> {
    let content: String = stmt.read("Content")?;
    let markdown = comrak::markdown_to_html(&content, markdown_options);
    let is_story: i64 = stmt.read("IsStory")?;
    let date: String = stmt.read("Date")?;
    let date_formatted = super::storage_datetime_as_display(&date)?;
    
    Ok(BlogPost { 
        author: stmt.read("Author")?, 
        author_username: stmt.read("Author_Username")?, 
        key: stmt.read("Key").unwrap_or("".into()), 
        title: stmt.read("Title")?, 
        content: markdown,
        date: date_formatted, 
        is_story: is_story > 0, 
        word_count: stmt.read("WordCount").unwrap_or(0), 
        comment_count: stmt.read("CommentCount").unwrap_or(0),
        comments: None })
}

pub fn comment_from_statement(stmt: &Statement, markdown_options: &comrak::ComrakOptions) -> Result<BlogComment, Box<dyn std::error::Error>> {
    let content: String = stmt.read("Content")?;
    let markdown = comrak::markdown_to_html(&content, markdown_options);
    let date: String = stmt.read("Date")?;
    let date_formatted = super::storage_datetime_as_display(&date)?;

    Ok(BlogComment { 
        id: stmt.read("Id")?, 
        author: stmt.read("Author")?, 
        date: date_formatted, 
        content: markdown,
        owned: false })
}