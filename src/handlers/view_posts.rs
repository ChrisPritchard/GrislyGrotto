use super::prelude::*;

use crate::data;

#[get("/")]
async fn latest(tmpl: Data<Tera>) -> impl Responder {
    let posts = data::get_latest_posts(0, "aquinas".to_string()).await.unwrap();
    
    let mut context = tera::Context::new();
    context.insert("posts", &posts);

    let html = tmpl.render("latest", &context).expect("template rendering failed");
    HttpResponse::Ok().body(html)
}

#[get("/post/{key}")]
async fn single(key: Path<String>, tmpl: Data<Tera>) -> impl Responder {
    let post = data::get_single_post(key.to_string(), "aquinas".to_string()).await.unwrap();

    if post.is_none() {
        return HttpResponse::NotFound().body("not found");
    }

    let mut post = post.unwrap();
    post.key = key.to_string();
    
    let mut context = tera::Context::new();
    context.insert("post", &post);

    let html = tmpl.render("single", &context).expect("template rendering failed");
    HttpResponse::Ok().body(html)
}
