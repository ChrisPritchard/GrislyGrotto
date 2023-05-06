use regex::Regex;

use super::{prelude::*, *};

pub async fn add_post(username: &str, title: &str, content: &str, is_story: bool) -> Result<()> {
    let date = current_datetime_for_storage();

    let mut key = title.to_ascii_lowercase().replace(" ", "-");
    let key_regex = Regex::new("[^A-Za-z0-9 -]+").unwrap();
    key = key_regex.replace_all(&key, "").into_owned();

    let word_count_regex = Regex::new("<[^>]*>").unwrap();
    let no_html = &word_count_regex.replace_all(content, "").into_owned();
    let word_count = no_html.split(" ").count() as i64;

    let is_story = if is_story { 1 } else { 0 };

    let connection = db()?;
    let mut stmt = connection.prepare(sql::INSERT_POST)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, username.into()), 
        (2, key.into()),
        (3, title.into()),
        (4, date.into()),
        (5, content.into()),
        (6, word_count.into()),
        (7, is_story.into()),])?;

    let _ = stmt.next()?;

    Ok(())
}