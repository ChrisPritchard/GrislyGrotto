use super::{prelude::*, *};

pub async fn validate_user(username: &str, password: &str) -> Result<bool> {
    let connection = db()?;
    let mut stmt = connection.prepare(sql::SELECT_USER_PASSWORD_HASH)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, username.to_string().into()),])?;

    if let Ok(State::Done) = stmt.next() {
        return Ok(false);
    }

    let hash: String = stmt.read("Password")?;
    let matches = argon2::verify_encoded(&hash, password.as_bytes()).unwrap();

    Ok(matches)
}
