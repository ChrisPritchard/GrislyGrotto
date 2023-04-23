use super::{prelude::*, *};

use crate::model::*;

pub async fn get_latest_posts(page: usize, current_user: String) -> Result<Vec<BlogPost>> {
    let connection = db()?;

    let mut stmt = connection.prepare(sql::SELECT_LATEST_POSTS)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, current_user.into()), 
        (2, 5.into()), 
        (3, (page as i64 * 5).into())])?;

    let mut final_result = Vec::new();
    let markdown_options = markdown_options();	

    while let Ok(State::Row) = stmt.next() {
        let post = mapping::post_from_statement(&stmt, &markdown_options)?;
        final_result.push(post);
    }

    Ok(final_result)
}

pub async fn get_single_post(key: String, current_user: String) -> Result<Option<BlogPost>> {
    let connection = db()?;

    let mut stmt = connection.prepare(sql::SELECT_SINGLE_POST)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, key.clone().into()), 
        (2, current_user.into())])?;

    if let Ok(State::Done) = stmt.next() {
        return Ok(None);
    }

    let markdown_options = markdown_options();
    let mut post = mapping::post_from_statement(&stmt, &markdown_options)?;

    let mut comments = Vec::new();
    
    let mut stmt = connection.prepare(sql::SELECT_COMMENTS)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, key.clone().into())])?;

    while let Ok(State::Row) = stmt.next() {
        let comment = mapping::comment_from_statement(&stmt, &markdown_options)?;
        comments.push(comment);
    }

    post.comments = Some(comments);
    Ok(Some(post))
}
