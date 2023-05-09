
use crate::data;

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

#[get("editor/exists/{key}")]
async fn key_exists(key: Query<String>, session: Session) -> WebResponse {
    if session.get::<String>("current_user").unwrap_or(None).is_none() {
        return Err(WebError::Forbidden);
    }

    let exists = data::editor::key_exists(key.as_str()).await?;
    json(exists)
}

#[derive(Deserialize)]
struct EditorForm {
    title: String,
    content: String,
    is_story: bool,
    is_draft: bool,
}

#[post("/editor/new")]
async fn create_new_post(form: Form<EditorForm>, session: Session) -> WebResponse {
    let current_user = session.get("current_user").unwrap_or(None);
    if current_user.is_none() {
        return Err(WebError::Forbidden);
    }
    let current_user: String = current_user.unwrap();

    if form.title.len() == 0 || form.content.len() == 0 || form.content.len() < 100 {
        return Err(WebError::BadRequest("invalid post".into()));
    }

    let result = data::editor::add_post(&current_user, &form.title, &form.content, form.is_story, form.is_draft).await?;
    if result.is_none() {
        return Err(WebError::BadRequest("post with this key already exists".into()));
    }

    let key = result.unwrap();

    redirect(format!("/post/{}", key))
}