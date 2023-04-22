use serde::Deserialize;

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

#[derive(Deserialize)]
struct CommentForm {
    author: String,
    content: String,
}

#[post("/post/{key}/comment")]
async fn comment(key: Path<String>, form: Form<CommentForm>) -> Either<HttpResponse, Redirect> {

    // TODO: proper error messages, if not handled by js
    if form.author.len() == 0 || form.content.len() == 0 {
        return Either::Left(HttpResponse::BadRequest().body("invalid comment"))
    }
    
    let result = data::add_comment(key.to_string(), form.author.clone(), form.content.clone()).await;
    if  result.is_err() {
        // TODO: better error handling / logging
        return Either::Left(HttpResponse::InternalServerError().body("something went wrong"))
    }

    let path = format!("/post/{}#comments", key.to_string());
    Either::Right(Redirect::to(path).see_other()) // will change from post to get
}