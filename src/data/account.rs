use rand::RngCore;

use super::{prelude::*, *};

pub async fn validate_user(username: &str, password: &str) -> Result<bool> {
    let connection = db()?;
    let mut stmt = connection.prepare(sql::SELECT_USER_PASSWORD_HASH)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, username.to_string().into()),])?;

    if stmt.next()? == State::Done {
        return Ok(false);
    }

    let hash: String = stmt.read("Password")?;
    let matches = argon2::verify_encoded(&hash, password.as_bytes()).unwrap();

    Ok(matches)
}

pub async fn update_user_password(username: &str, new_password: &str) -> Result<()> {
    let mut rng = rand::thread_rng();
    let new_salt: &mut [u8] = &mut [0; 32];
    rng.fill_bytes(new_salt);
    let new_hash = argon2::hash_encoded(new_password.as_bytes(), new_salt, &argon2::Config::default())?;

    let connection = db()?;
    let mut stmt = connection.prepare(sql::UPDATE_USER_PASSWORD_HASH)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, new_hash.to_string().into()),
        (2, username.to_string().into()),])?;

    let _ = stmt.next()?;
    Ok(())
}

pub async fn get_user_display_name(username: &str) -> Result<Option<String>> {
    let connection = db()?;
    let mut stmt = connection.prepare(sql::SELECT_USER_DISPLAY_NAME)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, username.to_string().into()),])?;

    if stmt.next()? == State::Done {
        return Ok(None);
    }

    let display_name: String = stmt.read("DisplayName")?;
    Ok(Some(display_name))
}

pub async fn update_user_display_name(username: &str, new_display_name: &str) -> Result<()> {
    let connection = db()?;
    let mut stmt = connection.prepare(sql::UPDATE_USER_DISPLAY_NAME)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, new_display_name.to_string().into()),
        (2, username.to_string().into()),])?;

    let _ = stmt.next()?;
    Ok(())
}