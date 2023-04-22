use super::{prelude::*, *};

pub async fn add_comment(key: String, author: String, content: String) -> Result<(), Error> {
    let date = format!("{}", chrono::offset::Local::now().format("%Y-%m-%d %H:%M:%S"));

    let connection = db()?;
    let mut stmt = connection.prepare(sql::INSERT_COMMENT)?;
    stmt.bind_iter::<_, (_, Value)>([
        (1, author.clone().into()), 
        (2, date.into()),
        (3, content.into()),
        (4, key.into()),])?;

    Ok(())
}