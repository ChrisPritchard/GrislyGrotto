use regex::Regex;

use super::{prelude::*, *};

pub async fn key_exists(key: &str) -> Result<bool> {
    let connection = db()?;
    let mut stmt = connection.prepare(sql::SELECT_EXISTING_KEY)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, key.into()),])?;
    Ok(stmt.next()? == State::Row)
}

pub async fn add_post(username: &str, title: &str, content: &str, is_story: bool, is_draft: bool) -> Result<Option<String>> {
    
    let mut key = title.to_ascii_lowercase().replace(" ", "-");
    let key_regex = Regex::new("[^A-Za-z0-9 -]+").unwrap();
    key = key_regex.replace_all(&key, "").into_owned();
    
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

pub async fn update_post(key: &str, title: &str, content: &str, is_story: bool, is_draft: bool) -> Result<()> {
    
    let connection = db()?;
    
    let word_count_regex = Regex::new("<[^>]*>").unwrap();
    let no_html = &word_count_regex.replace_all(content, "").into_owned();
    let word_count = no_html.split(" ").count() as i64;

    let is_story = if is_story { 1 } else { 0 };
    let title = if is_draft { format!("[DRAFT] {}", title) } else { title.to_string() };

    if !is_draft {
        let mut stmt = connection.prepare(sql::SELECT_EXISTING_KEY)?;
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
                (5, is_story.into()),])?;

            let _ = stmt.next()?;
        }

        return Ok(());
    }
    
    let mut stmt = connection.prepare(sql::UPDATE_POST)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, title.into()),
        (2, content.into()),
        (3, word_count.into()),
        (4, is_story.into()),])?;

    let _ = stmt.next()?;

    Ok(())
}