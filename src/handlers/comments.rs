use super::prelude::*;

use crate::data;

#[derive(Deserialize)]
struct CommentForm {
    author: String,
    content: String,
}

#[post("/post/{key}/comment")]
async fn add_comment(key: Path<String>, form: Form<CommentForm>) -> Either<HttpResponse, Redirect> {

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