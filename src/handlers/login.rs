use super::prelude::*;

#[get("/login")]
async fn login_page(tmpl: Data<Tera>, session: Session) -> impl Responder {
    let context = super::default_tera_context(session);
    let html = tmpl.render("login", &context).expect("template rendering failed");
    HttpResponse::Ok().body(html)
}

#[post("/login")]
async fn try_login(tmpl: Data<Tera>, session: Session) -> impl Responder {
    let context = super::default_tera_context(session);
    let html = tmpl.render("login", &context).expect("template rendering failed");
    HttpResponse::Ok().body(html)
}