use std::collections::HashSet;

use super::{prelude::*, *};

use crate::data;

#[derive(Deserialize)]
struct CommentForm {
    author: String,
    content: String,
    category: String, // just used for honeypot
}

#[post("/post/{key}/comment")]
async fn add_comment(key: Path<String>, form: Form<CommentForm>, session: Session) -> WebResponse {

    if form.category != "user" || form.author.len() == 0 || form.content.len() == 0 || form.content.len() > 1000 {
        return Err(WebError::BadRequest("invalid comment".into()))
    }
    
    let existing_count = data::comments::comment_count(&key).await?;
    
    if existing_count.is_none() {
        return Err(WebError::NotFound);
    } else if existing_count.unwrap() >= 20 {
        return Err(WebError::BadRequest("invalid comment".into()))
    }

    let new_id = data::comments::add_comment(&key, &form.author, &form.content).await?;

    let mut owned_comments = session.get("owned_comments").unwrap_or(None).unwrap_or(HashSet::new());
    owned_comments.insert(new_id);
    let _ = session.insert("owned_comments", owned_comments);

    let path = format!("/post/{}#comments", key.to_string());
    Redirect(path)
}

#[get("/raw_comment/{id}")]
async fn raw_comment_content(id: Path<i64>, session: Session) -> WebResponse {
    let id = id.into_inner();
    let owned_comments: HashSet::<i64> = session.get("owned_comments").unwrap_or(None).unwrap_or(HashSet::new());
    if !owned_comments.contains(&id) {
        return Err(WebError::Forbidden)
    }

    let content = data::comments::comment_content(id).await?;

    match content {
        None => Err(WebError::NotFound),
        Some(c) => Ok(c)
    }
}

#[derive(Deserialize)]
struct UpdateCommentForm {
    content: String,
}

#[post("/edit_comment/{id}")]
async fn edit_comment(id: Path<i64>, form: Form<UpdateCommentForm>, session: Session) -> WebResponse {
    let id = id.into_inner();
    let owned_comments: HashSet::<i64> = session.get("owned_comments").unwrap_or(None).unwrap_or(HashSet::new());
    if !owned_comments.contains(&id) {
        return Err(WebError::Forbidden)
    }

    if form.content.len() == 0 || form.content.len() > 1000 {
        return Err(WebError::BadRequest("invalid content length".into()))
    }

    let new_content = data::comments::update_comment_content(id, &form.content).await?;
    Accepted(new_content)
}

#[post("/delete_comment/{id}")]
async fn delete_comment(id: Path<i64>, session: Session) -> WebResponse {
    let id = id.into_inner();
    let owned_comments: HashSet::<i64> = session.get("owned_comments").unwrap_or(None).unwrap_or(HashSet::new());
    if !owned_comments.contains(&id) {
        return Err(WebError::Forbidden)
    }

    let _ = data::comments::delete_comment(id).await?;    
    Accepted("comment deleted".into())
}