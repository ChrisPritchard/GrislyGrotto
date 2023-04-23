use super::prelude::*;

use crate::data;

#[derive(Deserialize)]
struct PageInfo {
    page: Option<usize>,
}

#[get("/")]
async fn latest_posts(tmpl: Data<Tera>, query: Query<PageInfo>) -> impl Responder {

    let page = query.page.unwrap_or(0);
    let posts = data::get_latest_posts(page, "aquinas".to_string()).await;
    if let Err(err) = posts {
        error!("error getting latest posts: {}", err);
        return HttpResponse::InternalServerError().body("something went wrong")
    } 
    let posts = posts.unwrap();

    let mut context = tera::Context::new();
    context.insert("posts", &posts);
    context.insert("page", &page);

    let html = tmpl.render("latest", &context).expect("template rendering failed");
    HttpResponse::Ok().body(html)
}

#[get("/post/{key}")]
async fn single_post(key: Path<String>, tmpl: Data<Tera>) -> impl Responder {
    let post = data::get_single_post(key.to_string(), "aquinas".to_string()).await;
    if let Err(err) = post {
        error!("error getting single post: {}", err);
        return HttpResponse::InternalServerError().body("something went wrong")
    } 
    let post = post.unwrap();

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
