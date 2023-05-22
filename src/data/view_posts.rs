use std::collections::HashSet;

use super::{prelude::*, *};

use crate::model::*;

pub async fn get_latest_posts(page: usize, current_user: &str) -> Result<Vec<BlogPost>> {
    let connection = db()?;

    let mut stmt = connection.prepare(sql::SELECT_LATEST_POSTS)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, current_user.into()), 
        (2, 5.into()), 
        (3, (page as i64 * 5).into())])?;

    let mut final_result = Vec::new();

    while stmt.next()? == State::Row {
        let post = mapping::post_from_statement(&stmt)?;
        final_result.push(post);
    }

    Ok(final_result)
}

pub async fn get_single_post(key: &str, current_user: &str, owned_comments: &HashSet<i64>) -> Result<Option<BlogPost>> {
    let connection = db()?;

    let mut stmt = connection.prepare(sql::SELECT_SINGLE_POST)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, key.clone().into()), 
        (2, current_user.into())])?;

    if stmt.next()? == State::Done {
        return Ok(None);
    }

    let mut post = mapping::post_from_statement(&stmt)?;

    let mut comments = Vec::new();
    
    let mut stmt = connection.prepare(sql::SELECT_COMMENTS)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, key.clone().into())])?;

    while stmt.next()? == State::Row {
        let comment = mapping::comment_from_statement(&stmt, owned_comments)?;
        comments.push(comment);
    }

    post.comments = Some(comments);
    Ok(Some(post))
}

pub async fn delete_post(key: &str, current_user: &str) -> Result<()> {
    let connection = db()?;

    let mut stmt = connection.prepare(sql::DELETE_POST)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, key.clone().into()), 
        (2, current_user.into())])?;

    let _ = stmt.next()?;

    Ok(())
}