use super::{prelude::*, *};

#[get("/style/{mode}")]
async fn set_style(mode: Path<String>, session: Session) -> WebResponse {

    if mode.to_string() == "light" {
        session.insert("style", "light".to_string()).ok();
    } else if mode.to_string() == "dark" {
        session.insert("style", "dark".to_string()).ok();
    } else {
        return Err(WebError::BadRequest("invalid style name".into()));
    }

    Ok("style set".into())
}