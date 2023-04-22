use sqlite::{Value, State};

use crate::model::*;

mod sql;
mod mapping;

const DATABASE_PATH: &str = "./grislygrotto.db";

fn markdown_options() -> comrak::ComrakOptions {
    let mut markdown_options = comrak::ComrakOptions::default();
    markdown_options.render.unsafe_ = true;

    markdown_options
}

pub async fn get_latest_posts(page: i64, current_user: String) -> Result<Vec<BlogPost>, sqlite::Error> {
    let connection = sqlite::open(DATABASE_PATH)?;

    let mut stmt = connection.prepare(sql::SELECT_LATEST_POSTS)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, current_user.into()), 
        (2, 5.into()), 
        (3, (page * 5).into())])?;

    let mut final_result = Vec::new();
    let markdown_options = markdown_options();	

    while let Ok(State::Row) = stmt.next() {
        let post = mapping::post_from_statement(&stmt, &markdown_options)?;
        final_result.push(post);
    }

    Ok(final_result)
}

pub async fn get_single_post(key: String, current_user: String) -> Result<Option<BlogPost>, sqlite::Error> {
    let connection = sqlite::open(DATABASE_PATH)?;

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
