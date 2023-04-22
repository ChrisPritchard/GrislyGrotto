use super::prelude::*;

use crate::data;

#[get("/")]
async fn latest(tmpl: Data<Tera>) -> impl Responder {
    let posts = data::get_latest_posts(0, "aquinas".to_string()).await.unwrap();
    
    let mut context = tera::Context::new();
    context.insert("posts", &posts);

    let html = tmpl.render("index", &context).expect("template rendering failed");
    HttpResponse::Ok().body(html)
}