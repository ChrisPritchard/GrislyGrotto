use std::collections::HashSet;

use super::prelude::*;

use crate::data;

#[derive(Deserialize)]
struct CommentForm {
    author: String,
    content: String,
    category: String, // just used for honeypot
}

#[post("/post/{key}/comment")]
async fn add_comment(key: Path<String>, form: Form<CommentForm>, session: Session) -> Either<HttpResponse, Redirect> {

    if form.category != "user" || form.author.len() == 0 || form.content.len() == 0 || form.content.len() > 1000 {
        return Either::Left(HttpResponse::BadRequest().body("invalid comment"))
    }
    
    let existing_count = data::comments::comment_count(&key).await;
    if let Err(err) = existing_count {
        error!("error checking existing comment count: {}", err);
        return Either::Left(HttpResponse::InternalServerError().body("something went wrong"))
    } 
    
    let existing_count = existing_count.unwrap();
    
    if existing_count.is_none() {
        return Either::Left(HttpResponse::NotFound().body("not found"));
    } else if existing_count.unwrap() >= 20 {
        return Either::Left(HttpResponse::BadRequest().body("invalid comment"))
    }

    let result = data::comments::add_comment(&key, &form.author, &form.content).await;
    if let Err(err) = result {
        error!("error adding comment to database: {}", err);
        return Either::Left(HttpResponse::InternalServerError().body("something went wrong"))
    }

    let new_id = result.unwrap();

    let mut owned_comments = session.get("owned_comments").unwrap_or(None).unwrap_or(HashSet::new());
    owned_comments.insert(new_id);
    let _ = session.insert("owned_comments", owned_comments);

    let path = format!("/post/{}#comments", key.to_string());
    Either::Right(Redirect::to(path).see_other()) // 'see other' will make the request to the new path a GET
}

#[get("/raw_comment/{id}")]
async fn raw_comment_content(id: Path<i64>, session: Session) -> impl Responder {
    let id = id.into_inner();
    // let owned_comments: HashSet::<i64> = session.get("owned_comments").unwrap_or(None).unwrap_or(HashSet::new());
    // if !owned_comments.contains(&id) {
    //     return HttpResponse::Forbidden().body("forbidden")
    // }

    let content = data::comments::comment_content(id).await;
    if let Err(err) = content {
        error!("error getting raw comment content: {}", err);
        return HttpResponse::InternalServerError().body("something went wrong")
    }
    let content = content.unwrap();

    match content {
        None => HttpResponse::NotFound().body("not found"),
        Some(c) => HttpResponse::Ok().body(c)
    }
}

#[derive(Deserialize)]
struct UpdateCommentForm {
    content: String,
}

#[post("/edit_comment/{id}")]
async fn edit_comment(id: Path<i64>, form: Form<UpdateCommentForm>, session: Session) -> impl Responder {
    let id = id.into_inner();
    // let owned_comments: HashSet::<i64> = session.get("owned_comments").unwrap_or(None).unwrap_or(HashSet::new());
    // if !owned_comments.contains(&id) {
    //     return HttpResponse::Forbidden().body("forbidden")
    // }

    if form.content.len() == 0 || form.content.len() > 1000 {
        return HttpResponse::BadRequest().body("invalid content length")
    }

    let result = data::comments::update_comment_content(id, &form.content).await;
    if let Err(err) = result {
        error!("error updating comment: {}", err);
        return HttpResponse::InternalServerError().body("something went wrong")
    }
    let new_content = result.unwrap();
    HttpResponse::Accepted().body(new_content)
}

#[post("/delete_comment/{id}")]
async fn delete_comment(id: Path<i64>, session: Session) -> impl Responder {
    let id = id.into_inner();
    // let owned_comments: HashSet::<i64> = session.get("owned_comments").unwrap_or(None).unwrap_or(HashSet::new());
    // if !owned_comments.contains(&id) {
    //     return HttpResponse::Forbidden().body("forbidden")
    // }

    let result = data::comments::delete_comment(id).await;
    if let Err(err) = result {
        error!("error deleting comment: {}", err);
        return HttpResponse::InternalServerError().body("something went wrong")
    }
    
    HttpResponse::Accepted().body("comment deleted")
}