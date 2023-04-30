use super::{prelude::*, *};

pub async fn add_comment(key: &str, author: &str, content: &str) -> Result<i64> {
    let date = current_datetime_for_storage();

    let connection = db()?;
    let mut stmt = connection.prepare(sql::INSERT_COMMENT)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, author.clone().into()), 
        (2, date.into()),
        (3, content.into()),
        (4, key.into()),])?;

    let _ = stmt.next();

    let mut stmt = connection.prepare(sql::GET_LAST_COMMENT_ID)?;
    let _ = stmt.next();
    
    let id: i64 = stmt.read("Id")?;

    Ok(id)
}

pub async fn comment_count(key: &str) -> Result<Option<i64>> {
    let connection = db()?;
    let mut stmt = connection.prepare(sql::SELECT_COMMENT_COUNT)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, key.into()),])?;

    if let Ok(State::Done) = stmt.next() {
        return Ok(None);
    }

    let count: i64 = stmt.read("Count")?;

    Ok(Some(count))
}