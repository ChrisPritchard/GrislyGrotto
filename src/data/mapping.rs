use std::collections::HashSet;

use super::{prelude::*, *};

use crate::model::*;

pub fn post_from_statement(stmt: &Statement) -> Result<BlogPost> {
    let content: String = stmt.read("Content").unwrap_or("".into());
    let markdown = if content.len() > 0 { markdown_to_html(&content) } else { content };
    let is_story: i64 = stmt.read("IsStory").unwrap_or(0);
    let date: String = stmt.read("Date")?;
    let date_formatted = storage_datetime_as_display(&date)?;

    let title: String = stmt.read("Title")?;
    let (is_draft, title) = if title.starts_with("[DRAFT] ") { (true, title.chars().skip(8).collect()) } else { (false, title) };
    
    Ok(BlogPost { 
        author: stmt.read("Author")?, 
        author_username: stmt.read("Author_Username").unwrap_or("".into()), 
        key: stmt.read("Key").unwrap_or("".into()), 
        title: title.into(), 
        content: markdown,
        date: date_formatted, 
        is_story: is_story > 0, 
        is_draft,
        word_count: stmt.read("WordCount").unwrap_or(0), 
        comment_count: stmt.read("CommentCount").unwrap_or(0),
        comments: None })
}

pub fn raw_post_from_statement(stmt: &Statement) -> Result<EditPost> {
    let is_story: i64 = stmt.read("IsStory").unwrap_or(0);
    let title: String = stmt.read("Title")?;
    let (is_draft, title) = if title.starts_with("[DRAFT] ") { (true, title.chars().skip(8).collect()) } else { (false, title) };
    
    Ok(EditPost { 
        title: title.into(), 
        content: stmt.read("Content")?,
        is_story: is_story > 0, 
        is_draft, })
}

pub fn comment_from_statement(stmt: &Statement, owned_comments: &HashSet<i64>) -> Result<BlogComment> {
    let content: String = stmt.read("Content")?;
    let markdown = markdown_to_html(&content);
    let date: String = stmt.read("Date")?;
    let date_formatted = storage_datetime_as_display(&date)?;
    let id = stmt.read("Id")?;

    Ok(BlogComment { 
        id, 
        author: stmt.read("Author")?, 
        date: date_formatted, 
        content: markdown,
        owned: owned_comments.contains(&id) })
}

const MONTHS: [&str; 13] = ["", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

pub fn month_count_from_statement(stmt: &Statement) -> Result<MonthCount> {
    let date: String = stmt.read("Month")?;
    let year = (&date[..4]).to_string();
    
    let month = (&date[5..]).to_string();
    let index = month.parse::<usize>()?;
    let month = MONTHS[index].to_string();

    Ok(MonthCount { 
        year, month, count: stmt.read("Count")? })
}

pub fn index_of_month(month_name: &str) -> String {
    let mut index = -1;
    for month in MONTHS {
        index += 1;
        if month.to_lowercase() == month_name.to_lowercase() {
            return format!("{:0>2}", index);
        }
    }
    return "INVALID".into() // invalid month, will result in no data
}

pub fn prev_next_month(month_name: &str) -> Option<(String, String)> {
    let mut index = 0;
    for month in MONTHS {
        if month.to_lowercase() == month_name.to_lowercase() {
            if index == 1 {
                return Some((MONTHS[MONTHS.len()-1].into(),MONTHS[index+1].into()))
            } else if index == MONTHS.len()-1 {
                return Some((MONTHS[index-1].into(),MONTHS[1].into()))
            } else {
                return Some((MONTHS[index-1].into(),MONTHS[index+1].into()))
            }
        }
        index += 1;
    }
    None
}