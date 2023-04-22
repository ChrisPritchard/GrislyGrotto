use sqlite::Statement;

use crate::model::*;

pub fn post_from_statement(stmt: &Statement, markdown_options: &comrak::ComrakOptions) -> Result<BlogPost, sqlite::Error> {
    let content: String = stmt.read("Content")?;
    let markdown = comrak::markdown_to_html(&content, markdown_options);
    let is_story: i64 = stmt.read("IsStory")?;
    
    Ok(BlogPost { 
        author: stmt.read("Author")?, 
        author_username: stmt.read("Author_Username")?, 
        key: stmt.read("Key").or(Ok("".into()))?, 
        title: stmt.read("Title")?, 
        content: markdown,
        date: stmt.read("Date")?, 
        is_story: is_story > 0, 
        word_count: stmt.read("WordCount").or(Ok(0))?, 
        comment_count: stmt.read("CommentCount").or(Ok(0))?,
        comments: None })
}

pub fn comment_from_statement(stmt: &Statement, markdown_options: &comrak::ComrakOptions) -> Result<BlogComment, sqlite::Error> {
    let content: String = stmt.read("Content")?;
    let markdown = comrak::markdown_to_html(&content, markdown_options);

    Ok(BlogComment { 
        id: stmt.read("Id")?, 
        author: stmt.read("Author")?, 
        date: stmt.read("Date")?, 
        content: markdown,
        owned: false })
}