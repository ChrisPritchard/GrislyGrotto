
use super::{prelude::*, *};

#[get("/editor/new")]
async fn new_post(tmpl: Data<Tera>, session: Session) -> WebResponse {
    let context = super::default_tera_context(&session);
    let html = tmpl.render("editor", &context).expect("template rendering failed");
    ok(html)
}
