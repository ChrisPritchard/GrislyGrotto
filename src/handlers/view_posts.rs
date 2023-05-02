use std::collections::HashSet;

use super::prelude::*;
use anyhow::Result;

use crate::{data, handlers::WebError};

#[derive(Deserialize)]
struct PageInfo {
    page: Option<usize>,
}

#[get("/")]
async fn latest_posts(tmpl: Data<Tera>, query: Query<PageInfo>, session: Session) -> Result<HttpResponse, WebError> {

    let page = query.page.unwrap_or(0);
    let posts = data::view_posts::get_latest_posts(page, "aquinas").await?;

    let mut context = super::default_tera_context(&session);
    context.insert("posts", &posts);
    context.insert("page", &page);

    let html = tmpl.render("latest", &context).expect("template rendering failed");
    Ok(HttpResponse::Ok().body(html))
}

#[get("/post/{key}")]
async fn single_post(key: Path<String>, tmpl: Data<Tera>, session: Session) -> Result<HttpResponse, WebError> {
    let owned_comments = session.get("owned_comments").unwrap_or(None).unwrap_or(HashSet::new());

    let post = data::view_posts::get_single_post(&key, "aquinas", &owned_comments).await?;

    if post.is_none() {
        return Err(WebError::NotFound)
    }

    let mut post = post.unwrap();
    post.key = key.to_string();
    
    let mut context = super::default_tera_context(&session);
    context.insert("post", &post);

    let html = tmpl.render("single", &context).expect("template rendering failed");
    Ok(HttpResponse::Ok().body(html))
}
