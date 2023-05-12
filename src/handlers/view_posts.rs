use super::{prelude::*, *};

use crate::data;

#[derive(Deserialize)]
struct PageInfo {
    page: Option<usize>,
}

#[get("/")]
async fn latest_posts(tmpl: Data<Tera>, query: Query<PageInfo>, session: Session) -> WebResponse {
    let current_user = session.get("current_user")?.unwrap_or(Some("".to_string())).unwrap();

    let page = query.page.unwrap_or(0);
    let posts = data::view_posts::get_latest_posts(page, &current_user).await?;

    let mut context = super::default_tera_context(&session)?;
    context.insert("posts", &posts);
    context.insert("page", &page);

    let html = tmpl.render("latest", &context)?;
    ok(html)
}

#[get("/post/{key}")]
async fn single_post(key: Path<String>, tmpl: Data<Tera>, session: Session) -> WebResponse {
    let current_user = session.get("current_user")?.unwrap_or(Some("".to_string())).unwrap();
    let owned_comments = session.get("owned_comments")?.unwrap_or(HashSet::new());

    let post = data::view_posts::get_single_post(&key, &current_user, &owned_comments).await?;

    if post.is_none() {
        return Err(WebError::NotFound)
    }

    let mut post = post.unwrap();
    post.key = key.to_string();
    
    let mut context = super::default_tera_context(&session)?;
    context.insert("post", &post);

    if post.author_username == current_user {
        context.insert("own_blog", &true);
    }

    let html = tmpl.render("single", &context)?;
    ok(html)
}

#[post("/delete/{key}")]
async fn delete_post(key: Path<String>, session: Session) -> WebResponse {
    let current_user = session.get("current_user")?.unwrap_or(None);
    if current_user.is_none() {
        return Err(WebError::Forbidden);
    }
    let current_user: String = current_user.unwrap();

    let _ = data::view_posts::delete_post(&key.into_inner(), &current_user).await?;

    redirect("/".into())
}