use super::prelude::*;

#[get("/about")]
async fn about_page(tmpl: Data<Tera>, session: Session) -> impl Responder {
    let context = super::default_tera_context(&session);
    let html = tmpl.render("about", &context).expect("template rendering failed");
    HttpResponse::Ok().body(html)
}