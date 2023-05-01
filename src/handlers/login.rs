use crate::data;

use super::prelude::*;

#[get("/login")]
async fn login_page(tmpl: Data<Tera>, session: Session) -> impl Responder {
    let context = super::default_tera_context(&session);
    let html = tmpl.render("login", &context).expect("template rendering failed");
    HttpResponse::Ok().body(html)
}

#[derive(Deserialize)]
struct LoginForm {
    username: String,
    password: String,
    category: String, // just used for honeypot
}

#[post("/login")]
async fn try_login(form: Form<LoginForm>, tmpl: Data<Tera>, session: Session) -> Either<HttpResponse, Redirect> {

    if form.category != "user" || form.username.len() == 0 || form.password.len() == 0 {
        return Either::Left(HttpResponse::BadRequest().body("invalid comment"))
    }

    let valid = data::login::validate_user(&form.username, &form.password).await;
    if let Err(err) = valid {
        error!("error validating credentials: {}", err);
        return Either::Left(HttpResponse::InternalServerError().body("something went wrong"))
    } 
    let valid = valid.unwrap();

    let mut context = super::default_tera_context(&session);
    if !valid {
        context.insert("error", "invalid username and/or password");
    } else {
        let _ = session.insert("current_user", form.username.clone());
        return Either::Right(Redirect::to("/").see_other()) // 'see other' will make the request to the new path a GET
    }

    let html = tmpl.render("login", &context).expect("template rendering failed");
    Either::Left(HttpResponse::Ok().body(html))
}