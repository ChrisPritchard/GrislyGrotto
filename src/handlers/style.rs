use super::prelude::*;

#[get("/style/{mode}")]
async fn set_style(mode: Path<String>, session: Session) -> impl Responder {

    if mode.to_string() == "light" {
        session.insert("style", "light".to_string()).ok();
    } else if mode.to_string() == "dark" {
        session.insert("style", "dark".to_string()).ok();
    } else {
        return HttpResponse::BadRequest().body("invalid style name");
    }

    HttpResponse::Accepted().body("style set")
}