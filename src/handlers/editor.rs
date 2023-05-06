
use super::{prelude::*, *};

#[get("/editor/new")]
async fn new_post(tmpl: Data<Tera>, session: Session) -> WebResponse {
    if session.get::<String>("current_user").unwrap_or(None).is_none() {
        return Err(WebError::Forbidden);
    }

    let context = super::default_tera_context(&session);
    let html = tmpl.render("editor", &context).expect("template rendering failed");
    ok(html)
}

#[post("/editor/new")]
async fn create_new_post(session: Session) -> WebResponse {
    if session.get::<String>("current_user").unwrap_or(None).is_none() {
        return Err(WebError::Forbidden);
    }

    ok("test".into())
}