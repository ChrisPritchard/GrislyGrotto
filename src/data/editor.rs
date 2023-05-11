use regex::Regex;

use crate::model::EditPost;

use super::{prelude::*, *};

fn get_key_from_title(title: &str) -> String {
    let mut key = title.to_ascii_lowercase().replace(" ", "-");
    let key_regex = Regex::new("[^A-Za-z0-9 -]+").unwrap();
    key = key_regex.replace_all(&key, "").into_owned();
    key
}

pub async fn similar_title_exists(title: &str) -> Result<bool> {
    let key = get_key_from_title(title);

    let connection = db()?;

    let mut stmt = connection.prepare(sql::SELECT_EXISTING_KEY)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, key.into()),])?;
    Ok(stmt.next()? == State::Row)
}

pub async fn add_post(username: &str, title: &str, content: &str, is_story: bool, is_draft: bool) -> Result<Option<String>> {
    
    let key = get_key_from_title(title);
    
    let connection = db()?;

    let mut stmt = connection.prepare(sql::SELECT_EXISTING_KEY)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, key.clone().into()),])?;

    if stmt.next()? == State::Row {
        return Ok(None) // a post already exists with this key
    }
    
    let date = current_datetime_for_storage();

    let word_count_regex = Regex::new("<[^>]*>").unwrap();
    let no_html = &word_count_regex.replace_all(content, "").into_owned();
    let word_count = no_html.split(" ").count() as i64;

    let is_story = if is_story { 1 } else { 0 };

    let title = if is_draft { format!("[DRAFT] {}", title) } else { title.to_string() };

    let mut stmt = connection.prepare(sql::INSERT_POST)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, username.into()), 
        (2, key.clone().into()),
        (3, title.into()),
        (4, date.into()),
        (5, content.into()),
        (6, word_count.into()),
        (7, is_story.into()),])?;

    let _ = stmt.next()?;

    Ok(Some(key.clone()))
}

pub async fn get_post_for_edit(key: &str, current_user: &str) -> Result<Option<EditPost>> {
    let connection = db()?;

    let mut stmt = connection.prepare(sql::SELECT_RAW_POST)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, key.clone().into()), 
        (2, current_user.into())])?;

    if stmt.next()? == State::Done {
        return Ok(None);
    }

    let post = mapping::raw_post_from_statement(&stmt)?;
    Ok(Some(post))
}

pub async fn update_post(key: &str, title: &str, content: &str, is_story: bool, is_draft: bool) -> Result<()> {
    
    let connection = db()?;
    
    let word_count_regex = Regex::new("<[^>]*>").unwrap();
    let no_html = &word_count_regex.replace_all(content, "").into_owned();
    let word_count = no_html.split(" ").count() as i64;

    let is_story = if is_story { 1 } else { 0 };
    let title = if is_draft { format!("[DRAFT] {}", title) } else { title.to_string() };

    if !is_draft {
        let mut stmt = connection.prepare(sql::SELECT_IF_DRAFT)?;
        stmt.bind::<&[(_, Value)]>(&[
            (1, key.clone().into()),])?;

        if stmt.next()? == State::Row { // was draft, now isnt so update the date
            let date = current_datetime_for_storage();
            let mut stmt = connection.prepare(sql::UPDATE_POST_WITH_DATE)?;
            stmt.bind::<&[(_, Value)]>(&[
                (1, title.into()),
                (2, date.into()),
                (3, content.into()),
                (4, word_count.into()),
                (5, is_story.into()),
                (6, key.clone().into()),])?;

            let _ = stmt.next()?;

            return Ok(());
        }
    }
    
    let mut stmt = connection.prepare(sql::UPDATE_POST)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, title.into()),
        (2, content.into()),
        (3, word_count.into()),
        (4, is_story.into()),
        (5, key.clone().into()),])?;

    let _ = stmt.next()?;

    Ok(())
}